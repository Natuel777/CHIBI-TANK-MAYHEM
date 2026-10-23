using UnityEngine;

public class CommonCannonBullet : ShooteableObject
{
    private Vector3 _velocity;
    private const float _Gravity = -9.81f;
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
        _velocity += new Vector3(0f, _Gravity, 0f) * Time.deltaTime;
        transform.position += _velocity * Time.deltaTime;
        currentLifetime -= Time.deltaTime;

        if(currentLifetime <= 0)
            CannonBulletFactory.Instance.Return(this);
    }

    private void OnTriggerEnter(Collider other)
    {
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
