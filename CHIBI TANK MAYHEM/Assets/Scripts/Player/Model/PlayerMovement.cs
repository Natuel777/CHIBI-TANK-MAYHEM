using UnityEngine;

//Movimiento físico del tanque basado en "velocidad objetivo": el input no mueve el tanque
//directamente, define una velocidad/giro DESEADOS, y el tanque acelera o frena hacia ese objetivo
//cada frame de física. Es el mismo enfoque que usan la mayoría de los vehicle controllers (autos,
//tanques) en juegos, porque es estable por construcción: la velocidad nunca puede superar maxSpeed
//ni el giro maxTurnRate, a diferencia de aplicar fuerzas (Newtons) directamente sobre el Rigidbody,
//que puede acumular energía sin control si la fuerza es grande o la inercia del objeto es chica.
public class PlayerMovement : IInputInitialize
{
    private Transform _transform;
    private Transform _meshTransform;
    private bool _initialized = false;
    private Vector2 _moveInput;

    private float _maxSpeed, _acceleration, _deceleration;
    private float _maxTurnRate, _turnAcceleration;
    private float _pitchTiltAmount, _pitchTiltSmoothTime, _pitchTiltDeadzone;
    private Rigidbody _rb;

    private float _currentSpeed;
    private float _currentTurnRate;
    private float _previousSpeed;    //_currentSpeed del frame anterior, se usa solo para calcular cuánto cambió la velocidad (aceleración instantánea) y así animar el cabeceo
    private float _currentTiltAngle; //ángulo de cabeceo visual ACTUAL del mesh (grados), se desliza suavemente hacia un objetivo y de vuelta a 0
    private float _tiltVelocity;     //velocidad interna que usa SmoothDamp para suavizar _currentTiltAngle (la mantiene entre frames, no la tocamos a mano)

    public PlayerMovement(Transform playerTransform, Transform meshTransform,
                                                    Rigidbody rb,
                                                    float maxSpeed,
                                                    float acceleration,
                                                    float deceleration,
                                                    float maxTurnRate,
                                                    float turnAcceleration,
                                                    float pitchTiltAmount,
                                                    float pitchTiltSmoothTime,
                                                    float pitchTiltDeadzone,
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
        _pitchTiltAmount = pitchTiltAmount;         //cuántos grados de cabeceo se generan por cada (m/s²) de aceleración instantánea
        _pitchTiltSmoothTime = pitchTiltSmoothTime; //tiempo aproximado (s) que tarda el cabeceo en llegar a su objetivo, vía SmoothDamp
        _pitchTiltDeadzone = pitchTiltDeadzone;     //aceleraciones menores a esto (m/s²) se ignoran para el cabeceo (filtra micro-cambios al girar)

        //centerOfMass define alrededor de qué punto rota físicamente el Rigidbody (por ejemplo, al
        //chocar). No es el pivote del giro por input, que se hace con MoveRotation sobre el transform
        //completo, no alrededor de un punto interno.
        _rb.centerOfMass = centerOfMassOffset;

        //Congelamos la rotación física en X (cabeceo) y Z (vuelco lateral): el tanque NUNCA puede
        //volcar ni rodar por un choque, una rampa o cualquier torque externo. El giro en Y lo maneja
        //este script con MoveRotation cada frame, y el cabeceo visual se dibuja aparte sobre el mesh
        //(ApplyPitchTilt) sin tocar la física — por eso la física no necesita libertad de rotación en
        //X/Z para nada, ni siquiera para el balanceo.
        _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
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

        UpdateForwardSpeed(); //1. decide hacia qué velocidad de avance ir
        UpdateTurnRate();     //2. decide hacia qué velocidad de giro ir
        ApplyVelocity();      //3. mueve y rota el Rigidbody según esos dos valores
        ApplyPitchTilt();     //4. anima el balanceo visual del mesh según cuánto cambió la velocidad
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
        //la dirección contraria a la actual (ej. iba adelante y ahora se pide atrás).
        bool decelerating = Mathf.Abs(targetSpeed) < Mathf.Abs(_currentSpeed) ||
                            !Mathf.Approximately(Mathf.Sign(targetSpeed), Mathf.Sign(_currentSpeed));
        float rate = decelerating ? _deceleration : _acceleration;

        _currentSpeed = Mathf.MoveTowards(_currentSpeed, targetSpeed, rate * Time.fixedDeltaTime);
    }

    //PASO 2 — Giro.
    //Mismo esquema de velocidad-objetivo que el avance, pero para la rotación en Y (grados/segundo).
    //Al ser independiente del avance, el tanque puede girar exactamente igual estando quieto (pivot
    //turn, como gira un tanque real usando las orugas en direcciones opuestas) que mientras se mueve.
    private void UpdateTurnRate()
    {
        float targetTurnRate = _moveInput.x * _maxTurnRate;
        _currentTurnRate = Mathf.MoveTowards(_currentTurnRate, targetTurnRate, _turnAcceleration * Time.fixedDeltaTime);
    }

    //PASO 3 — Aplicar al Rigidbody.
    //Avance: se escribe DIRECTAMENTE la velocidad lineal horizontal (forward * _currentSpeed) sobre
    //linearVelocity, preservando el componente Y actual (así la gravedad / caídas siguen funcionando
    //normalmente, no las pisamos). Esto es distinto de aplicar una FUERZA: acá directamente le decimos
    //al Rigidbody "tu velocidad horizontal ahora es esta", sin pasar por F = m·a.
    //Giro: se usa MoveRotation en vez de tocar transform.rotation directamente, porque MoveRotation
    //le avisa al motor de física del movimiento (así las colisiones durante el giro se resuelven bien,
    //en vez de que el tanque atraviese paredes si giramos "a mano" sin que la física se entere).
    private void ApplyVelocity()
    {
        Vector3 forwardVelocity = _transform.forward * _currentSpeed;
        Vector3 velocity = _rb.linearVelocity;
        _rb.linearVelocity = new Vector3(forwardVelocity.x, velocity.y, forwardVelocity.z);

        float turnThisStep = _currentTurnRate * Time.fixedDeltaTime; //grados a rotar EN ESTE frame (velocidad * tiempo)
        _rb.MoveRotation(_rb.rotation * Quaternion.Euler(0f, turnThisStep, 0f));
    }

    //PASO 4 — Balanceo (cabeceo) visual.
    //Esto es pura estética, no física real: rota el MESH (no el Rigidbody) en X para simular que el
    //tanque "siente" la aceleración. Como es visual, nunca puede desestabilizar ni volcar el tanque
    //real (que ya está protegido por FreezeRotationX/Z).
    private void ApplyPitchTilt()
    {
        if(_meshTransform == null) return;

        //Aceleración instantánea REAL en m/s²: cuánto cambió _currentSpeed desde el frame anterior,
        //dividido el tiempo. Es lo que efectivamente está pasando ahora (arrancar, frenar), que puede
        //diferir de la aceleración configurada.
        float accelerationThisStep = (_currentSpeed - _previousSpeed) / Time.fixedDeltaTime;
        _previousSpeed = _currentSpeed;

        //Zona muerta: si la aceleración es chica (ej. la velocidad target bajó un poquito al girar en
        //movimiento), la tratamos como 0 para que esos micro-cambios NO disparen el cabeceo. Solo los
        //arranques/frenados reales (aceleración fuerte) superan el umbral y producen balanceo.
        if(Mathf.Abs(accelerationThisStep) < _pitchTiltDeadzone)
            accelerationThisStep = 0f;

        //Ángulo deseado del cabeceo, proporcional a la aceleración. El signo negativo hace que ACELERAR
        //levante el morro (nariz arriba) y FRENAR lo hunda
        float targetTilt = -accelerationThisStep * _pitchTiltAmount;

        //SmoothDamp en vez de MoveTowards: MoveTowards avanza a velocidad constante y "clava" en el
        //objetivo con una esquina dura; SmoothDamp interpola tipo resorte amortiguado — arranca y
        //frena suave, sin sobresaltos. _tiltVelocity es su estado interno (lo mantiene entre frames).
        //Cuando targetTilt vuelve a 0 (sin aceleración), el mesh se asienta suavemente solo.
        _currentTiltAngle = Mathf.SmoothDamp(_currentTiltAngle, targetTilt, ref _tiltVelocity, _pitchTiltSmoothTime, Mathf.Infinity, Time.fixedDeltaTime);

        _meshTransform.localRotation = Quaternion.Euler(_currentTiltAngle, 0f, 0f);
    }
}
