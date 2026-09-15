using UnityEngine;

public class CommonTurretBullet : ShooteableObject
{
    private Vector3 _direction;
    
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
        }

        else if(other.gameObject.CompareTag("TankHead"))
        {
            Player player = other.GetComponentInParent<Player>();
            if(player != null)
                player.healthModel?.HeadTakeDamage(initialDamage);
        }

        else if(other.gameObject.CompareTag("TankTurret"))
        {
            Player player = other.GetComponentInParent<Player>();
            if(player != null)
                player.healthModel?.TurretTakeDamage(initialDamage);
        }

        else if(other.gameObject.CompareTag("TankTrailLFT"))
        {
            Player player = other.GetComponentInParent<Player>();
            if(player != null)
                player.healthModel?.TrailLFTTakeDamage(initialDamage);
        }

        else if(other.gameObject.CompareTag("TankTrailRGT"))
        {
            Player player = other.GetComponentInParent<Player>();
            if(player != null)
                player.healthModel?.TrailRGTTakeDamage(initialDamage);
        }

        else if(other.TryGetComponent(out CommonChibiSoldier commonChibiSoldier))
        {
            commonChibiSoldier.healthModel?.TakeDamage(initialDamage);
        }

        TurretBulletFactory.Instance.Return(this);
    }
}