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
}
