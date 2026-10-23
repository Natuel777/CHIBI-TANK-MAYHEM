using UnityEngine;

public class CommonTurretBullet : ShooteableObject
{
    private Vector3 _direction;
    private Quaternion ImpactRotation => Quaternion.LookRotation(-_direction);

    public override void Shoot(Vector3 direction)
    {
        _direction = direction;
    }

    private void Update()
    {
        transform.position += _direction * speed * Time.deltaTime;
        currentLifetime -= Time.deltaTime;

        if(currentLifetime <= 0)
            TurretBulletFactory.Instance.Return(this);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("TankBody"))
        {
            Player player = other.GetComponentInParent<Player>();
            
            if(player != null)
                player.healthModel?.BodyTakeDamage(initialDamage);

            if(TryResolveService())
            {
                GenericPS ps = _PSFactory.Create(PSType.MetalTurretBulletImpact, transform.position, ImpactRotation);
                ps.Initialize();
            }
        }

        else if(other.gameObject.CompareTag("TankHead"))
        {
            Player player = other.GetComponentInParent<Player>();
            
            if(player != null)
                player.healthModel?.HeadTakeDamage(initialDamage);

            if(TryResolveService())
            {
                GenericPS ps = _PSFactory.Create(PSType.MetalTurretBulletImpact, transform.position, ImpactRotation);
                ps.Initialize();
            }
        }

        else if(other.gameObject.CompareTag("TankTurret"))
        {
            Player player = other.GetComponentInParent<Player>();
            
            if(player != null)
                player.healthModel?.TurretTakeDamage(initialDamage);

            if(TryResolveService())
            {
                GenericPS ps = _PSFactory.Create(PSType.MetalTurretBulletImpact, transform.position, ImpactRotation);
                ps.Initialize();
            }
        }

        else if(other.gameObject.CompareTag("TankTrailLFT"))
        {
            Player player = other.GetComponentInParent<Player>();
            
            if(player != null)
                player.healthModel?.TrailLFTTakeDamage(initialDamage);

            if(TryResolveService())
            {
                GenericPS ps = _PSFactory.Create(PSType.MetalTurretBulletImpact, transform.position, ImpactRotation);
                ps.Initialize();
            }
        }

        else if(other.gameObject.CompareTag("TankTrailRGT"))
        {
            Player player = other.GetComponentInParent<Player>();
            
            if(player != null)
                player.healthModel?.TrailRGTTakeDamage(initialDamage);

            if(TryResolveService())
            {
                GenericPS ps = _PSFactory.Create(PSType.MetalTurretBulletImpact, transform.position, ImpactRotation);
                ps.Initialize();
            }
        }

        else if(other.TryGetComponent(out CommonChibiSoldier commonChibiSoldier))
        {
            commonChibiSoldier.healthModel?.TakeDamage(initialDamage);
        }

        else if(TryResolveService())
        {
            GenericPS ps = _PSFactory.Create(PSType.WallTurretBulletImpact, GetSurfacePoint(other), ImpactRotation);
            ps.Initialize();
        }

        TurretBulletFactory.Instance.Return(this);
    }

    private Vector3 GetSurfacePoint(Collider other)
    {
        Vector3 direction = _direction.normalized;
        float lookBack = speed * Time.fixedDeltaTime * 2f;
        Ray ray = new Ray(transform.position - direction * lookBack, direction);

        if(other.Raycast(ray, out RaycastHit hit, lookBack * 2f))
            return hit.point;

        return transform.position;
    }
}