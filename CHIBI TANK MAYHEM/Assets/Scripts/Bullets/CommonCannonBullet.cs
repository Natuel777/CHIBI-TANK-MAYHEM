using UnityEngine;

public class CommonCannonBullet : ShooteableObject
{
    private Vector3 _velocity;
    private const float _Gravity = -9.81f;

    //El Rigidbody del prefab es kinematic y sin useGravity: el motor de físicas no mueve esta bala y Physics.gravity no la afecta.
    //Se mueve sola en Update(), y la caída sale de _Gravity. Por eso este es el valor que tiene que usar quien calcule la
    //trayectoria (ver ChibiSoldierCaptureTargetCannonBehaviour).
    public override float Gravity => _Gravity;

    public override void ResetState()
    {
        base.ResetState();
        _velocity = Vector3.zero;
    }

    public override void Shoot(Vector3 direction)
    {
        _velocity = direction * speed;
    }

    private void Update()
    {
        //Gravedad real: es una ACELERACIÓN (m/s²), así que se acumula sobre la velocidad frame a
        //frame, no se suma directo a la dirección. Sumarla antes de escalar por speed (como estaba)
        //la multiplicaba por speed también, volviéndola gigante e inmediata en vez de una caída progresiva.
        _velocity += new Vector3(0f, _Gravity, 0f) * Time.deltaTime;
        transform.position += _velocity * Time.deltaTime;

        currentLifetime -= Time.deltaTime;

        if(currentLifetime <= 0)
            CannonBulletFactory.Instance.Return(this);
    }

    private void OnTriggerEnter(Collider other)
    {
        //El trigger de detección del capture target no es un blanco: sin esto la bala se desarma apenas nace dentro de su radio.
        if(other.TryGetComponent(out ChibiSoldierCaptureTarget _)) return;

        if(other.TryGetComponent(out CommonChibiSoldier commonChibiSoldier))
        {
            commonChibiSoldier.healthModel?.TakeDamage(initialDamage);
        }

        if(TryResolveService())
        {
            GenericPS ps = _PSFactory.Create(PSType.BoomText, transform.position + Vector3.up, Quaternion.identity);
            ps.Initialize();
        } 
        
        CannonBulletFactory.Instance.Return(this);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(TryResolveService())
        {
            GenericPS ps = _PSFactory.Create(PSType.BoomText, transform.position + Vector3.up, Quaternion.identity);
            ps.Initialize();
        } 
        
        CannonBulletFactory.Instance.Return(this);
    }
}
