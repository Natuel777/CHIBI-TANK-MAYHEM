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
public class PlayerMovement : IInputInitialize
{
    private Transform _transform;
    private Transform _meshTransform;
    private bool _initialized = false;
    private Vector2 _moveInput;

    private float _maxSpeed, _acceleration, _deceleration;
    private float _maxTurnRate, _turnAcceleration;
    private float _pitchTiltAmount, _pitchTiltSmoothTime;
    private float _groundCheckDistance;
    private LayerMask _groundMask;
    private Rigidbody _rb;

    private float _currentSpeed;
    private float _currentTurnRate;
    private bool _isDeceleratingThisStep;  //true si este frame el avance está frenando/invirtiendo (no acelerando) — lo calcula UpdateForwardSpeed y lo lee ApplyPitchTilt
    private bool _isChangingSpeedThisStep; //true si _currentSpeed todavía no llegó a targetSpeed (sigue en rampa). Falso en velocidad crucero constante, aunque haya input
    private bool _isGrounded;              //true si el tanque está apoyado en el suelo (último resultado del raycast). En el aire, no forzamos la velocidad — la gravedad manda
    private Vector3 _groundNormal;         //normal del suelo bajo el tanque: define el plano sobre el que avanzar en rampas
    private Vector3 _lastCheckedPosition;  //posición del tanque la última vez que se hizo el raycast de suelo — para no repetirlo si no se movió (ver CheckGrounded)
    private float _currentTiltAngle;       //ángulo de cabeceo visual ACTUAL del mesh (grados), se desliza suavemente hacia un objetivo y de vuelta a 0
    private float _tiltVelocity;           //velocidad interna que usa SmoothDamp para suavizar _currentTiltAngle (la mantiene entre frames, no la tocamos a mano)

    public PlayerMovement(Transform playerTransform, Transform meshTransform,
                                                    Rigidbody rb,
                                                    float maxSpeed,
                                                    float acceleration,
                                                    float deceleration,
                                                    float maxTurnRate,
                                                    float turnAcceleration,
                                                    float pitchTiltAmount,
                                                    float pitchTiltSmoothTime,
                                                    float groundCheckDistance,
                                                    LayerMask groundMask,
                                                    Vector3 centerOfMassOffset)
    {
        _transform = playerTransform;
        _meshTransform = meshTransform;
        _rb = rb;
        _maxSpeed = maxSpeed;
        _acceleration = acceleration;
        _deceleration = deceleration;
        _maxTurnRate = maxTurnRate;
        _turnAcceleration = turnAcceleration;
        _pitchTiltAmount = pitchTiltAmount;             //ángulo (grados) del cabeceo mientras dura una rampa de arranque/frenado
        _pitchTiltSmoothTime = pitchTiltSmoothTime;     //tiempo aproximado (s) que tarda el cabeceo en llegar a su objetivo, vía SmoothDamp
        _groundCheckDistance = groundCheckDistance;     //hasta qué distancia hacia abajo se considera que el tanque está "apoyado" en el suelo
        _groundMask = groundMask;                       //qué capas cuentan como suelo para el raycast (evita detectarse a sí mismo)

        //centerOfMass define alrededor de qué punto rota físicamente el Rigidbody (choques, vuelco en
        //rampas, etc.). Bajarlo un poco ayuda a que el tanque sea más estable y no se dé vuelta fácil.
        _rb.centerOfMass = centerOfMassOffset;

        //NO congelamos ninguna rotación: la física es libre de inclinar el tanque según el terreno.
        //El giro por input se aplica sobre el eje propio del tanque (ver ApplyVelocity), así convive
        //con esa inclinación en vez de pelearse con ella.
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

        CheckGrounded();      //0. ¿está apoyado en el suelo? define si controlamos el avance o lo maneja la gravedad
        UpdateForwardSpeed(); //1. decide hacia qué velocidad de avance ir
        UpdateTurnRate();     //2. decide hacia qué velocidad de giro ir
        ApplyVelocity();      //3. mueve y rota el Rigidbody según esos dos valores y el estado del suelo
        ApplyPitchTilt();     //4. anima el balanceo visual del mesh si hay una rampa de arranque/frenado en curso
    }

    //PASO 0 — Ground check.
    //Un raycast corto hacia abajo desde el centro del tanque: si golpea suelo dentro de la distancia,
    //el tanque está "apoyado" (_isGrounded) y guardamos la normal de esa superficie (_groundNormal),
    //que usamos para avanzar SIGUIENDO la pendiente. Si no golpea nada, el tanque está en el aire
    //(cayó de un precipicio, saltó una rampa) y dejamos que la gravedad/física manejen la caída sola.
    //
    //Optimización: el raycast solo se relanza si el tanque efectivamente cambió de posición desde el
    //último chequeo. Si no se movió (input suelto y ya en reposo, por ejemplo), _isGrounded/_groundNormal
    //de la última vez siguen siendo válidos y no hace falta gastar un Physics.Raycast por gusto.
    private void CheckGrounded()
    {
        Vector3 currentPosition = _rb.position;

        if(currentPosition == _lastCheckedPosition) return;
        _lastCheckedPosition = currentPosition;

        //El origen se levanta un poco (0.1) para no arrancar el ray justo en el borde del collider.
        Vector3 origin = _rb.worldCenterOfMass + Vector3.up * 0.1f;

        if(Physics.Raycast(origin, Vector3.down, out RaycastHit hit, _groundCheckDistance + 0.1f, _groundMask, QueryTriggerInteraction.Ignore))
        {
            _isGrounded = true;
            _groundNormal = hit.normal;
        }
        else
        {
            _isGrounded = false;
            _groundNormal = Vector3.up;
        }
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
        //GIRO: se rota sobre el eje "arriba" PROPIO del tanque (_transform.up), no el del mundo. En
        //terreno plano son iguales, pero en una rampa el eje propio está inclinado con la pendiente,
        //así que el tanque gira "sobre el plano de la rampa" en vez de atravesar el suelo. MoveRotation
        //le avisa a la física del giro (para que las colisiones se resuelvan bien).
        float turnThisStep = _currentTurnRate * Time.fixedDeltaTime;
        _rb.MoveRotation(_rb.rotation * Quaternion.AngleAxis(turnThisStep, Vector3.up));

        //AVANCE:
        if(_isGrounded)
        {
            //En el suelo: avanzamos siguiendo la PENDIENTE. Proyectar el forward del tanque sobre el
            //plano del terreno (usando su normal) da una dirección que sube/baja rampas correctamente,
            //sin "clavarse" contra la cuesta ni despegarse en las bajadas.
            Vector3 slopeForward = Vector3.ProjectOnPlane(_transform.forward, _groundNormal).normalized;
            Vector3 desiredVelocity = slopeForward * _currentSpeed;

            //Se preserva la componente de la velocidad ALINEADA con la normal del suelo (normalmente
            //casi 0 estando apoyado), para no pisar micro-ajustes verticales de la física de contacto.
            Vector3 currentVelocity = _rb.linearVelocity;
            Vector3 velocityAlongNormal = Vector3.Project(currentVelocity, _groundNormal);
            _rb.linearVelocity = desiredVelocity + velocityAlongNormal;
        }
        //En el AIRE: no tocamos linearVelocity. La gravedad y el momento que ya llevaba manejan la
        //caída de forma natural (nada de empuje horizontal artificial mientras cae de un precipicio).
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
