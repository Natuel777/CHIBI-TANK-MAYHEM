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
            if(other.TryGetComponent<Player>(out Player player))
                player.healthModel?.BodyTakeDamage(initialDamage);
        }

        else if(other.gameObject.CompareTag("TankHead"))
        {
            if(other.TryGetComponent<Player>(out Player player))
                player.healthModel?.HeadTakeDamage(initialDamage);
        }

        else if(other.gameObject.CompareTag("TankTurret"))
        {
            if(other.TryGetComponent<Player>(out Player player))
                player.healthModel?.TurretTakeDamage(initialDamage);
        }

        else
        {
            //if(other.TryGetComponent<Player>(out Player player))
            //    player.healthModel?.TurretTakeDamage();
        }

        TurretBulletFactory.Instance.Return(this);
    }
}