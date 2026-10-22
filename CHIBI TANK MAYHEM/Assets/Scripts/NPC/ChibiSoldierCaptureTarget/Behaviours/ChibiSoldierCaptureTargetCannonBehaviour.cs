using UnityEngine;

public sealed class ChibiSoldierCaptureTargetCannonBehaviour : IShootable, IBehaviours, IServiceConsumer
{
    private Transform _yRotationCannonMesh, _xRotationCannonMesh, 
                    _targetTransform, _muzzle;
    private bool _active;
    private CannonBulletFactory _cannonBulletFactory;
    private readonly float _rotationSpeed, _shootCooldown;
    private float _shootTimer; 

    //Cuántas pasadas de refinado hace GetPitchError. Con 2 ya converge (se explica ahí); 3 deja margen.
    private const int _AimIterations = 3;

    //Datos de la bala que sale del pool. Shoot() los actualiza en cada disparo, y entre disparos se usan para ir inclinando
    //el cañón de a poco. Hasta el primer disparo valen 0: sin gravedad conocida el cañón apunta en línea recta al target.
    private float _bulletSpeed, _bulletGravity;

    public bool IsActive => _active;
    public Transform TargetTransform => _targetTransform;

    public ChibiSoldierCaptureTargetCannonBehaviour(Transform yRotationCannonMesh, Transform xRotationCannonMesh,
                                                    Transform muzzle, float rotationSpeed, float shootCooldown)
    {
        _yRotationCannonMesh = yRotationCannonMesh;
        _xRotationCannonMesh = xRotationCannonMesh;
        _muzzle = muzzle;
        _rotationSpeed = rotationSpeed;
        _shootCooldown = shootCooldown;
    }

    public void ArtificialUpdate()
    {
        if(!_active) return;

        if(_shootTimer > 0f) _shootTimer -= Time.deltaTime;

        if(_targetTransform == null) return;

        RotateYTowardsTarget();
        RotateXTowardsTarget();

        if(_shootTimer > 0f) return;

        Shoot();
        _shootTimer = _shootCooldown;
    }

    private void RotateYTowardsTarget()
    {
        Vector3 toTarget = Vector3.ProjectOnPlane(_targetTransform.position - _yRotationCannonMesh.position, Vector3.up);
        Vector3 barrelDirection = Vector3.ProjectOnPlane(_muzzle.up, Vector3.up);

        if(toTarget.sqrMagnitude < 0.0001f || barrelDirection.sqrMagnitude < 0.0001f) return;

        float angle = Vector3.SignedAngle(barrelDirection, toTarget, Vector3.up);
        float maxStep = _rotationSpeed * Time.deltaTime;

        _yRotationCannonMesh.Rotate(0f, Mathf.Clamp(angle, -maxStep, maxStep), 0f, Space.World);
    }

    private void RotateXTowardsTarget()
    {
        //El ángulo ya no es "mirar al target": es el que hace que la PARÁBOLA de la bala pase por el target (ver GetPitchError).
        //Entre disparos se calcula con los datos de la última bala, así el cañón ya viene inclinado cuando llega el siguiente tiro.
        float angle = GetPitchError(_bulletSpeed, _bulletGravity);
        float maxStep = _rotationSpeed * Time.deltaTime;
        _xRotationCannonMesh.Rotate(Mathf.Clamp(angle, -maxStep, maxStep), 0f, 0f, Space.Self);
    }

    //PROCESO DE CADA DISPARO:
    //  1. Se saca la bala del pool. Create() la devuelve activa, con el estado reseteado y parada en el muzzle.
    //  2. Se le leen los datos que definen su trayectoria: Speed (con qué rapidez sale) y Gravity (cuánto cae).
    //     El Rigidbody del prefab no cuenta: es kinematic y sin useGravity, o sea que el motor de físicas no mueve la bala.
    //     Se mueve sola en CommonCannonBullet.Update(), y es ahí donde se aplica la gravedad.
    //  3. Con esos datos se calcula la inclinación exacta del cañón (GetPitchError) y se gira todo lo que falta, sin límite
    //     de velocidad: en el instante del disparo el cañón tiene que apuntar justo donde dice la cuenta.
    //  4. Al girar, el muzzle cambió de lugar, así que se vuelve a parar la bala sobre él.
    //  5. La bala sale por muzzle.up con velocidad = Speed, y desde ahí la gravedad la va curvando hasta el target.
    //Ojo: esto resuelve la inclinación (eje X). Que el tiro acierte también depende de que el giro en Y ya haya llegado al target.
    public void Shoot()
    {
        if(_targetTransform == null) return;

        if(!TryResolveService()) return;

        ShooteableObject bullet = _cannonBulletFactory.Create(BulletType.CommonCannonBullet, _muzzle.position, _muzzle.rotation);

        _bulletSpeed = bullet.Speed;
        _bulletGravity = bullet.Gravity;

        _xRotationCannonMesh.Rotate(GetPitchError(_bulletSpeed, _bulletGravity), 0f, 0f, Space.Self);

        bullet.transform.SetPositionAndRotation(_muzzle.position, _muzzle.rotation);
        bullet.Shoot(_muzzle.up);
    }

    //Devuelve cuántos grados hay que girar el cañón sobre su eje X (con signo) para que la bala, al salir del muzzle,
    //describa una parábola que pase por el target. No modifica ningún transform: solo calcula.
    //
    //Se trabaja con la ELEVACIÓN: el ángulo del cañón contra el piso (0 = horizontal, positivo = hacia arriba, en radianes).
    //Depende solo de la distancia horizontal y la diferencia de altura al target, no de hacia dónde esté girado el cañón en Y,
    //por eso el pitch se resuelve aparte del yaw (RotateYTowardsTarget).
    //
    //La complicación: el ángulo que se necesita depende de desde dónde sale la bala, y sale del muzzle, que está a
    //muzzleDistance del pivote y se mueve al inclinar el cañón. O sea, el ángulo depende del muzzle y el muzzle depende del
    //ángulo. Se resuelve por aproximaciones sucesivas:
    //  - Se arranca con la recta pivote → target (sin gravedad, esa ya sería la respuesta).
    //  - En cada pasada se calcula dónde quedaría el muzzle con la elevación actual, qué elevación necesita la bala saliendo
    //    de AHÍ (SolveLaunchElevation) y se corrige la elevación hacia ese valor.
    //  - La corrección se divide por (1 + distancia del muzzle / distancia al target). Si el cañón sube, el muzzle sube con él
    //    y el target queda más abajo respecto del muzzle: la respuesta se mueve EN CONTRA del cambio. Corregir de una (sin dividir)
    //    se pasa de largo y oscila cuando el target está cerca; dividiendo es un paso del método de Newton y converge en 2 pasadas.
    //Se asume que el muzzle está sobre el eje del cañón, que es como está armado el prefab.
    private float GetPitchError(float bulletSpeed, float bulletGravity)
    {
        Vector3 pivot = _xRotationCannonMesh.position;
        Vector3 pitchAxis = _xRotationCannonMesh.right;
        Vector3 barrelDirection = _muzzle.up;

        //Rumbo horizontal del cañón (hacia dónde mira sobre el piso): la elevación se mide sobre este rumbo.
        Vector3 barrelHeading = Vector3.ProjectOnPlane(barrelDirection, Vector3.up);

        if(barrelHeading.sqrMagnitude < 0.0001f) return 0f;

        barrelHeading.Normalize();

        float muzzleDistance = Vector3.Distance(pivot, _muzzle.position);
        Vector3 targetPosition = _targetTransform.position;

        //Punto de partida: la recta del pivote al target.
        Vector3 pivotToTarget = targetPosition - pivot;
        float elevation = Mathf.Atan2(pivotToTarget.y, HorizontalLength(pivotToTarget));

        for(int i = 0; i < _AimIterations; i++)
        {
            //Dónde quedaría el muzzle con el cañón a esta elevación.
            Vector3 muzzlePosition = pivot + ElevationToDirection(barrelHeading, elevation) * muzzleDistance;
            Vector3 muzzleToTarget = targetPosition - muzzlePosition;

            //Qué elevación necesita la bala para llegar al target saliendo de ese muzzle.
            float requiredElevation = SolveLaunchElevation(HorizontalLength(muzzleToTarget), muzzleToTarget.y, bulletSpeed, bulletGravity);

            //Paso de Newton (ver arriba).
            float damping = 1f + muzzleDistance / Mathf.Max(muzzleToTarget.magnitude, 0.01f);
            elevation += (requiredElevation - elevation) / damping;
        }

        //De elevación (ángulo contra el piso) a rotación: cuántos grados hay que girar sobre el eje X para pasar de la
        //dirección actual del cañón a la deseada. Ambas direcciones están en el plano perpendicular a ese eje.
        return Vector3.SignedAngle(barrelDirection, ElevationToDirection(barrelHeading, elevation), pitchAxis);
    }

    //FÍSICA DEL TIRO PARABÓLICO
    //
    //Una bala que sale con rapidez v a un ángulo θ sobre la horizontal y solo sufre una aceleración constante hacia abajo g
    //se mueve así (x = distancia horizontal recorrida, y = altura ganada, t = tiempo):
    //    x(t) = v·cosθ·t                 en horizontal no hay aceleración: la velocidad es constante
    //    y(t) = v·sinθ·t − ½·g·t²        en vertical sube con v·sinθ y la gravedad la frena y la tira hacia abajo
    //
    //Se despeja t de la primera (t = x / (v·cosθ)) y se reemplaza en la segunda. Queda la trayectoria sin el tiempo:
    //    y = x·tanθ − g·x² / (2·v²·cos²θ)
    //
    //Con la identidad 1/cos²θ = 1 + tan²θ y llamando u = tanθ, es una ecuación de segundo grado en u:
    //    (g·x² / 2v²)·u² − x·u + (y + g·x² / 2v²) = 0
    //
    //La fórmula cuadrática da la solución:
    //    tanθ = ( v² ± √( v⁴ − g·(g·x² + 2·y·v²) ) ) / (g·x)
    //
    //Cómo leerla:
    //  - Lo que está dentro de la raíz (el discriminante) dice si el target se puede alcanzar:
    //      · negativo → ningún ángulo llega: el target está fuera del alcance de esa rapidez
    //      · cero     → hay un único ángulo: el target está justo en el borde del alcance
    //      · positivo → hay DOS ángulos que aciertan: uno bajo (tiro tenso, el "−") y uno alto (tiro de mortero, el "+")
    //  - Se usa el "−": la bala llega antes, vuela menos tiempo y el target casi no se mueve mientras tanto.
    //  - Si no se puede alcanzar se deja el discriminante en 0: sale el ángulo con el que la bala pasa lo más cerca posible
    //    del target, el mejor intento.
    //  - Se usa atan2 en vez de dividir por g·x para que no explote cuando el target está justo arriba o abajo (x = 0 da ±90°).
    //  - Sin gravedad (o sin rapidez conocida) la trayectoria es una recta, y el ángulo es el de la línea directa al target.
    //
    //Qué tan fiel es a la bala real: CommonCannonBullet no usa el motor de físicas, integra a mano frame a frame
    //(v += g·dt; p += v·dt). Eso cae apenas más que la parábola exacta (≈ ½·g·dt·t de más: unos 2 cm a 30 m con speed 100
    //y 60 fps), despreciable acá.
    //
    //horizontalDistance y heightDifference van del punto de salida al target. gravity va con signo (negativa = hacia abajo).
    //Devuelve la elevación en radianes.
    private static float SolveLaunchElevation(float horizontalDistance, float heightDifference, float speed, float gravity)
    {
        float g = -gravity;

        if(g < 0.0001f || speed < 0.0001f)
            return Mathf.Atan2(heightDifference, horizontalDistance);

        float speedSquared = speed * speed;
        float discriminant = speedSquared * speedSquared - g * (g * horizontalDistance * horizontalDistance + 2f * heightDifference * speedSquared);
        float root = Mathf.Sqrt(Mathf.Max(discriminant, 0f));

        return Mathf.Atan2(speedSquared - root, g * horizontalDistance);
    }

    private static float HorizontalLength(Vector3 vector) => new Vector2(vector.x, vector.z).magnitude;

    //Dirección con la elevación pedida, manteniendo el rumbo horizontal (heading, normalizado) que tiene el cañón.
    private static Vector3 ElevationToDirection(Vector3 heading, float elevation) => heading * Mathf.Cos(elevation) + Vector3.up * Mathf.Sin(elevation);

    public void Active(bool active) => _active = active;

    public ChibiSoldierCaptureTargetCannonBehaviour UpdateTarget(Transform targetTransform)
    {
        _targetTransform = targetTransform;
        return this;
    }

    public ChibiSoldierCaptureTargetCannonBehaviour EmptyTarget()
    {
        _targetTransform = null;
        return this;
    }

    public bool TryResolveService()
    {
        if(_cannonBulletFactory != null) return true;

        return ServiceLocator.Instance.TryGet(out _cannonBulletFactory);
    }
}
