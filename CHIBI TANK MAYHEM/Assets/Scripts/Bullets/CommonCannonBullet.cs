using UnityEngine;

public class CommonCannonBullet : ShooteableObject
{
    private Vector3 _velocity;
    private const float _Gravity = -9.81f;

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
}
