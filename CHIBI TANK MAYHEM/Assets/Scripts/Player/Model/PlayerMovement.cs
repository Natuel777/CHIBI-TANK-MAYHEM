using UnityEngine;

//Movimiento físico del tanque basado en "velocidad objetivo": el input no mueve el tanque
//directamente, define una velocidad/giro DESEADOS, y el tanque acelera o frena hacia ese objetivo
//cada frame de física. Es el mismo enfoque que usan la mayoría de los vehicle controllers (autos,
//tanques) en juegos, porque es estable por construcción: la velocidad nunca puede superar maxSpeed
//ni el giro maxTurnRate, a diferencia de aplicar fuerzas (Newtons) directamente sobre el Rigidbody,
//que puede acumular energía sin control si la fuerza es grande o la inercia del objeto es chica.
//
//Las rotaciones físicas quedan LIBRES (sin FreezeRotation): así el tanque se orienta solo con el
//terreno — sube rampas, baja pendientes, se inclina y cae de precipicios de forma realista. El
//control por input (avance y giro) se aplica RELATIVO a esa orientación física actual, no en ejes
//del mundo, para que todo conviva bien en terreno inclinado.
//
//SUSPENSIÓN: el chasis NO se apoya sobre su collider — FLOTA unos centímetros sobre el terreno,
//sostenido por resortes virtuales (raycasts + ley de Hooke) en las esquinas de las orugas. Esto es
//lo que hace al tanque todoterreno: un obstáculo chico (banquina, lomo de burro) pasa por DEBAJO del
//box collider sin generar colisión física, y solo comprime uno o dos resortes, que levantan esa
//esquina progresivamente. Antes, ese mismo obstáculo golpeaba una arista del box lejos del centro de
//masa y la física lo resolvía como un impulso puntual → torque enorme → el tanque volcaba o volaba.
//El collider sigue estando y sigue chocando normal contra paredes y objetos grandes; simplemente
//dejó de ser la pieza que toca el piso.
public class PlayerMovement : IInputInitialize
{
    private Transform _transform;
    private Transform _meshTransform;
    private Transform[] _suspensionPoints;
    private bool _initialized = false;
    private Vector2 _moveInput;

    private float _maxSpeed, _acceleration, _deceleration;
    private float _maxTurnRate, _turnAcceleration;
    private float _pitchTiltAmount, _pitchTiltSmoothTime;
    private float _suspensionRestLength, _suspensionStrength, _suspensionDampingRatio;
    private float _groundNormalSmoothing;
    private LayerMask _groundMask;
    private Rigidbody _rb;

    private float _currentSpeed;
    private float _currentTurnRate;
    private bool _isDeceleratingThisStep;  //true si este frame el avance está frenando/invirtiendo (no acelerando) — lo calcula UpdateForwardSpeed y lo lee ApplyPitchTilt
    private bool _isChangingSpeedThisStep; //true si _currentSpeed todavía no llegó a targetSpeed (sigue en rampa). Falso en velocidad crucero constante, aunque haya input
    private bool _isGrounded;              //true si AL MENOS un resorte de la suspensión está tocando suelo. En el aire, no forzamos la velocidad — la gravedad manda
    private Vector3 _groundNormal = Vector3.up; //normal PROMEDIO del suelo bajo las orugas (suavizada): define el plano sobre el que avanzar en rampas
    private float _currentTiltAngle;       //ángulo de cabeceo visual ACTUAL del mesh (grados), se desliza suavemente hacia un objetivo y de vuelta a 0
    private float _tiltVelocity;           //velocidad interna que usa SmoothDamp para suavizar _currentTiltAngle (la mantiene entre frames, no la tocamos a mano)

    public PlayerMovement(Transform playerTransform, Transform meshTransform,
                                                    Transform[] suspensionPoints,
                                                    Rigidbody rb,
                                                    float maxSpeed,
                                                    float acceleration,
                                                    float deceleration,
                                                    float maxTurnRate,
                                                    float turnAcceleration,
                                                    float pitchTiltAmount,
                                                    float pitchTiltSmoothTime,
                                                    float suspensionRestLength,
                                                    float suspensionStrength,
                                                    float suspensionDampingRatio,
                                                    float groundNormalSmoothing,
                                                    LayerMask groundMask,
                                                    Vector3 centerOfMassOffset)
    {
        _transform = playerTransform;
        _meshTransform = meshTransform;
        _suspensionPoints = suspensionPoints;           //anclajes de los resortes: 6 puntos (frente/medio/atrás x izq/der) en el fondo del chasis, sobre las orugas
        _rb = rb;
        _maxSpeed = maxSpeed;
        _acceleration = acceleration;
        _deceleration = deceleration;
        _maxTurnRate = maxTurnRate;
        _turnAcceleration = turnAcceleration;
        _pitchTiltAmount = pitchTiltAmount;             //ángulo (grados) del cabeceo mientras dura una rampa de arranque/frenado
        _pitchTiltSmoothTime = pitchTiltSmoothTime;     //tiempo aproximado (s) que tarda el cabeceo en llegar a su objetivo, vía SmoothDamp
        _suspensionRestLength = suspensionRestLength;   //recorrido total del resorte (m): hasta dónde "alcanza" cada rueda a buscar el suelo
        _suspensionStrength = suspensionStrength;       //fuerza máxima del resorte, en múltiplos del peso que le toca sostener a esa rueda
        _suspensionDampingRatio = suspensionDampingRatio; //amortiguación relativa a la crítica: 0 = rebota eterno, 1 = sin rebote
        _groundNormalSmoothing = groundNormalSmoothing; //qué tan rápido la normal del suelo se interpola hacia la nueva (más alto = transición más brusca)
        _groundMask = groundMask;                       //qué capas cuentan como suelo para el raycast (evita detectarse a sí mismo)

        //centerOfMass define alrededor de qué punto rota físicamente el Rigidbody (choques, vuelco en
        //rampas, etc.). Bajarlo un poco ayuda a que el tanque sea más estable y no se dé vuelta fácil.
        _rb.centerOfMass = centerOfMassOffset;

        //NO congelamos ninguna rotación: la física es libre de inclinar el tanque según el terreno.
        //El giro por input controla SOLO la guiñada y deja el cabeceo/alabeo en manos de la suspensión
        //(ver ApplyVelocity), así convive con esa inclinación en vez de pelearse con ella.
        _rb.constraints = RigidbodyConstraints.None;
    }

    public void Initialize(InputReader inputReader)
    {
        inputReader.Move += SetMoveInput;
        _initialized = true;
    }

    private void SetMoveInput(Vector2 inputs) { _moveInput = inputs; }

    public void ArtificialFixedUpdate()
    {
        if(!_initialized) return;

        ApplySuspension();    //0. los resortes sostienen el chasis, lo inclinan con el terreno y calculan _isGrounded/_groundNormal
        UpdateForwardSpeed(); //1. decide hacia qué velocidad de avance ir
        UpdateTurnRate();     //2. decide hacia qué velocidad de giro ir
        ApplyVelocity();      //3. mueve y rota el Rigidbody según esos dos valores y el estado del suelo
        ApplyPitchTilt();     //4. anima el balanceo visual del mesh si hay una rampa de arranque/frenado en curso
    }

    //PASO 0 — Suspensión.
    //Cada punto de anclaje tira un raycast corto hacia abajo. Si encuentra suelo dentro del recorrido
    //del resorte, empuja el chasis hacia arriba con una fuerza proporcional a cuánto está comprimido.
    //Esto cumple tres funciones a la vez:
    //  1. SOSTIENE el tanque flotando sobre el terreno, para que su collider no toque el piso.
    //  2. Lo INCLINA solo con el terreno: al aplicar fuerzas distintas en puntos distintos
    //     (AddForceAtPosition), la rueda más comprimida empuja más fuerte y eso GENERA el torque que
    //     cabecea o alabea el chasis. No hay que rotar nada a mano.
    //  3. Calcula _isGrounded y la normal promedio del suelo bajo las orugas.
    //
    //Nota: acá no se puede aplicar la optimización de "no relanzar el raycast si el tanque no se movió".
    //Los resortes tienen que aplicar fuerza en TODOS los pasos de física o el tanque se cae. De todas
    //formas, un puñado de raycasts por FixedUpdate es un costo despreciable.
    private void ApplySuspension()
    {
        //Si todavía no asignaste los 6 puntos en el Inspector, no hay nada que sostenga al tanque:
        //cortamos acá para no dividir por cero más abajo (Length == 0).
        if(_suspensionPoints == null || _suspensionPoints.Length == 0) return;

        //Arrancamos el frame asumiendo que el tanque está en el aire. Si al menos UN resorte
        //toca el piso más abajo, se pone en true de nuevo.
        _isGrounded = false;
        Vector3 normalSum = Vector3.zero; //vamos sumando la normal de cada resorte que tocó suelo, para promediarla al final
        int hitCount = 0;                 //cuántos resortes tocaron suelo este frame (para el promedio de arriba)

        //--- CUÁNTA FUERZA MÁXIMA puede dar CADA resorte ---
        //Peso total del tanque (mass * gravedad) repartido en partes iguales entre los N puntos.
        //Ej: tanque de 1000kg con 6 puntos → cada uno "le toca sostener" 1000/6 kg de peso.
        float weightPerPoint = _rb.mass * Physics.gravity.magnitude / _suspensionPoints.Length;
        //La fuerza máxima que puede dar el resorte es un MÚLTIPLO de ese peso (suspensionStrength).
        //Con strength=2, cada resorte puede empujar hasta el DOBLE de lo que necesita para
        //sostener su parte del peso → sobra fuerza para además frenar baches, no solo sostenerse quieto.
        float maxSpringForce = weightPerPoint * _suspensionStrength;

        //--- CUÁNTO FRENAR EL REBOTE (amortiguación) ---
        //Un resorte SOLO (sin amortiguador) rebotaría para siempre, como un colchón sin fricción.
        //El amortiguador le saca energía en cada rebote hasta que se queda quieto.
        //k = "dureza" del resorte, calculada a partir de la fuerza máxima y el recorrido.
        float springConstant = maxSpringForce / _suspensionRestLength;
        //La "amortiguación crítica" es el punto exacto donde el resorte se asienta SIN rebotar
        //ni una sola vez (fórmula estándar de física: 2 * raíz(dureza * masa)).
        float criticalDamping = 2f * Mathf.Sqrt(springConstant * (_rb.mass / _suspensionPoints.Length));
        //suspensionDampingRatio es un porcentaje de esa amortiguación crítica: 0 = nada de freno
        //(rebota siempre), 1 = freno total (se asienta sin rebotar), 0.5 = un rebotecito y listo.
        float damping = criticalDamping * _suspensionDampingRatio;

        //Repetimos este cálculo para CADA uno de los 6 puntos de suspensión (uno por rueda/oruga).
        for(int i = 0; i < _suspensionPoints.Length; i++)
        {
            if(_suspensionPoints[i] == null) continue; //por si falta asignar alguno en el Inspector, no rompe

            Vector3 origin = _suspensionPoints[i].position; //desde dónde sale el "rayo" de este resorte

            //Tiramos un rayo hacia abajo desde este punto, hasta suspensionRestLength de largo.
            //Si NO encuentra suelo en ese rango, este resorte está "en el aire" (estirado del todo,
            //sin tocar nada) y no aplica ninguna fuerza: pasamos al siguiente punto.
            if(!Physics.Raycast(origin, -_transform.up, out RaycastHit hit,
                                _suspensionRestLength, _groundMask, QueryTriggerInteraction.Ignore))
                continue;

            //Si llegamos acá, ESTE resorte sí tocó suelo.
            _isGrounded = true;
            hitCount++;
            normalSum += hit.normal; //sumamos su normal para el promedio de más abajo

            //hit.distance es qué tan lejos está el suelo (0 = pegado al punto, restLength = al límite).
            //compression es lo contrario, "qué tan aplastado" está el resorte: 0 = nada aplastado
            //(el suelo está lejos, casi al límite del rayo), 1 = totalmente aplastado (suelo pegado al punto).
            float compression = 1f - (hit.distance / _suspensionRestLength);

            //LEY DE HOOKE: la fuerza de un resorte es proporcional a cuánto está comprimido.
            //Un resorte muy aplastado empuja fuerte para volver a su largo natural; uno casi
            //estirado empuja poco. Por eso multiplicamos compression * la fuerza máxima posible.
            float springForce = compression * maxSpringForce;

            //Ahora medimos qué tan rápido se está moviendo el chasis VERTICALMENTE en este punto
            //(subiendo o bajando). GetPointVelocity da la velocidad real de ese punto del cuerpo
            //rígido (no solo el centro), y Dot con "arriba" nos deja solo la parte vertical.
            float verticalVelocity = Vector3.Dot(_rb.GetPointVelocity(origin), _transform.up);
            //Fuerza del amortiguador: siempre en sentido CONTRARIO al movimiento (por eso el signo
            //negativo). Si el punto está subiendo rápido, esta fuerza empuja hacia abajo para
            //frenarlo, y viceversa. "damping" (calculado arriba) es cuán fuerte frena.
            float damperForce = -verticalVelocity * damping;

            //Sumamos resorte + amortiguador para tener la fuerza final de este punto.
            //Mathf.Max(0f, ...) es importante: en la vida real un resorte solo puede EMPUJAR
            //(nunca puede "tirar" del chasis hacia el suelo). Sin este límite, si el tanque cae
            //rápido el amortiguador daría un número negativo enorme que lo mandaría volando.
            float totalForce = Mathf.Max(0f, springForce + damperForce);

            //Aplicamos esa fuerza hacia ARRIBA (_transform.up), justo en este punto del chasis
            //(no en el centro). Aplicar la fuerza EN EL PUNTO es lo que genera el giro/inclinación:
            //si un solo resorte empuja fuerte y los demás no, esa esquina se levanta más que las otras.
            _rb.AddForceAtPosition(_transform.up * totalForce, origin);
        }

        //Terminado el loop: promediamos las normales de todos los resortes que tocaron suelo (si
        //ninguno tocó, usamos "arriba" por defecto). Esa normal es hacia dónde "mira" el suelo en
        //promedio bajo el tanque, y se usa después para hacerlo avanzar siguiendo la pendiente.
        Vector3 targetNormal = hitCount > 0 ? (normalSum / hitCount).normalized : Vector3.up;
        //Slerp en vez de asignar directo: suaviza el cambio de normal a lo largo de varios frames,
        //para que el paso de piso plano a rampa sea gradual y no un salto brusco.
        _groundNormal = Vector3.Slerp(_groundNormal, targetNormal, _groundNormalSmoothing * Time.fixedDeltaTime);
    }

    //PASO 1 — Avance.
    //_moveInput.y * _maxSpeed es la velocidad que "querríamos tener ya mismo" si no hubiera inercia.
    //En vez de saltar a ese valor de golpe, MoveTowards lo acerca gradualmente: así se siente una
    //rampa de aceleración en vez de un movimiento instantáneo tipo teletransporte.
    //Se usan DOS tasas distintas a propósito: _acceleration cuando el tanque va "ganando" velocidad
    //en la dirección que ya llevaba, y _deceleration cuando frena o invierte el sentido
    private void UpdateForwardSpeed()
    {
        float targetSpeed = _moveInput.y * _maxSpeed;

        //decelerating = true si el objetivo pide MENOS velocidad en la misma dirección, o si pide
        //la dirección contraria a la actual (ej. iba adelante y ahora se pide atrás). Se guarda en
        //_isDeceleratingThisStep porque ApplyPitchTilt necesita el mismo criterio (no uno propio
        //basado en el delta de velocidad, que también se movería al girar por otras razones).
        bool decelerating = Mathf.Abs(targetSpeed) < Mathf.Abs(_currentSpeed) ||
                            !Mathf.Approximately(Mathf.Sign(targetSpeed), Mathf.Sign(_currentSpeed));
        _isDeceleratingThisStep = decelerating;

        //_isChangingSpeedThisStep: true solo mientras _currentSpeed TODAVÍA no llegó al objetivo (está
        //en plena rampa de aceleración/frenado). Una vez alcanzado targetSpeed (velocidad crucero
        //constante), pasa a false — así el cabeceo no queda "pegado" mientras avanzás sostenido.
        _isChangingSpeedThisStep = !Mathf.Approximately(_currentSpeed, targetSpeed);

        float rate = decelerating ? _deceleration : _acceleration;
        _currentSpeed = Mathf.MoveTowards(_currentSpeed, targetSpeed, rate * Time.fixedDeltaTime);
    }

    //PASO 2 — Giro.
    //Mismo esquema de velocidad-objetivo que el avance, pero para la rotación (grados/segundo).
    //Al ser independiente del avance, el tanque puede girar exactamente igual estando quieto (pivot
    //turn, como gira un tanque real usando las orugas en direcciones opuestas) que mientras se mueve.
    private void UpdateTurnRate()
    {
        float targetTurnRate = _moveInput.x * _maxTurnRate;
        _currentTurnRate = Mathf.MoveTowards(_currentTurnRate, targetTurnRate, _turnAcceleration * Time.fixedDeltaTime);
    }

    //PASO 3 — Aplicar al Rigidbody.
    private void ApplyVelocity()
    {
        //GIRO: antes era MoveRotation, que TELETRANSPORTA la rotación entera del cuerpo y por eso
        //peleaba contra la inclinación que le impone la suspensión (cada frame le borraba lo que los
        //resortes acababan de hacer). Ahora seteamos la velocidad angular en dos partes:
        //  - la componente de GUIÑADA (alrededor del eje vertical del MUNDO) la manda el input;
        //  - la de cabeceo/alabeo queda INTACTA, para que los resortes inclinen libremente el chasis.
        Vector3 angularVelocity = _rb.angularVelocity;
        Vector3 tiltAngularVelocity = angularVelocity - Vector3.Project(angularVelocity, Vector3.up);
        _rb.angularVelocity = tiltAngularVelocity + Vector3.up * (_currentTurnRate * Mathf.Deg2Rad);

        //En el AIRE no hay tracción: mandan la gravedad y el momento que el tanque ya traía (nada de
        //empuje horizontal artificial mientras cae de un precipicio).
        if(!_isGrounded) return;

        //Dirección de avance proyectada sobre el plano del terreno: en una cuesta la velocidad
        //horizontal baja por el coseno de la pendiente (subir cuesta arriba avanza menos), como
        //corresponde. Lo que sube o baja al tanque son los resortes, no esta velocidad.
        Vector3 slopeForward = Vector3.ProjectOnPlane(_transform.forward, _groundNormal).normalized;
        Vector3 desiredVelocity = slopeForward * _currentSpeed;

        //CLAVE: solo escribimos el plano horizontal (X y Z). La componente VERTICAL queda 100% en manos
        //de la gravedad y de los resortes. Como el código nunca inyecta velocidad hacia arriba, el
        //tanque NO PUEDE salir volando por un obstáculo: lo peor que pasa es que un resorte lo levante
        //suavemente. Antes se escribía el vector completo, y esa Y positiva era literalmente un
        //empujón hacia arriba al entrar en una rampa.
        _rb.linearVelocity = new Vector3(desiredVelocity.x, _rb.linearVelocity.y, desiredVelocity.z);
    }

    //PASO 4 — Balanceo (cabeceo) visual.
    //Esto es pura estética, no física real: rota el MESH (no el Rigidbody ni sus colliders) en X para
    //simular que el tanque "siente" la aceleración. Al aplicarse SOLO al mesh visual, no toca la
    //geometría de colisión, así que no puede generar bugs de colisión (trepar paredes, etc.).
    //
    //El cabeceo debe darse SOLO en tres casos: arrancar desde parado, invertir la dirección de avance,
    //o frenar — nunca por girar en movimiento (_moveInput.x no participa acá para nada) ni mientras se
    //avanza sostenido a velocidad crucero. Por eso NO se infiere a partir de "cuánto cambió _currentSpeed
    //este frame puntual" (ese delta puede tener ruido ajeno al avance en sí). En cambio, se usan dos
    //flags que ya calculó UpdateForwardSpeed en base pura al input/estado de avance:
    //  _isChangingSpeedThisStep → hay una rampa de aceleración/frenado en curso (no velocidad crucero)
    //  _isDeceleratingThisStep  → esa rampa es de frenado/inversión (no de arranque)
    private void ApplyPitchTilt()
    {
        if(_meshTransform == null) return;

        //Magnitud del cabeceo: constante mientras dure la rampa. Se apaga (vuelve a 0) apenas
        //_currentSpeed alcanza targetSpeed, aunque el input siga sostenido.
        //Signo: arrancando (no decelerating) el morro sube; frenando/invirtiendo, el morro baja.
        float targetTilt = 0f;
        if(_isChangingSpeedThisStep)
            targetTilt = _isDeceleratingThisStep ? _pitchTiltAmount : -_pitchTiltAmount;

        //SmoothDamp interpola tipo resorte amortiguado — arranca y frena suave, sin la esquina dura de
        //MoveTowards. _tiltVelocity es su estado interno. Cuando targetTilt vuelve a 0, se asienta solo.
        //Este tilt es LOCAL al mesh, así que se suma visualmente encima de cualquier inclinación física
        //del tanque por el terreno (que ya afecta al root), sin interferir con ella.
        _currentTiltAngle = Mathf.SmoothDamp(_currentTiltAngle, targetTilt, ref _tiltVelocity, _pitchTiltSmoothTime, Mathf.Infinity, Time.fixedDeltaTime);

        _meshTransform.localRotation = Quaternion.Euler(_currentTiltAngle, 0f, 0f);
    }
}
