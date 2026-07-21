using UnityEngine;

public class PlayerTurretShoot : IInputInitialize, IShootable
{
    private Transform _turretMuzzleTransform;
    private bool _initialized = false, _isShooting = false;
    private BulletType _currentBulletType = BulletType.CommonTurretBullet;
    private float _fireRate;
    private float _fireCooldown;

    public PlayerTurretShoot(Transform turretMuzzleTransform, float fireRate, float fireCooldown)
    {
        _turretMuzzleTransform = turretMuzzleTransform;
        _fireRate = fireRate;
        _fireCooldown = fireCooldown;
    }

    public void Initialize(InputReader inputReader)
    {
        inputReader.ShootGun += SetShooting;
        _initialized = true;
    }

    public void SetShooting(bool isShooting)
    {
        _isShooting = isShooting;
    }

    public void ArtificialUpdate()
    {
        if(!_initialized || !_isShooting) return;

        _fireCooldown -= Time.deltaTime;

        if(_fireCooldown <= 0f)
        {
            Shoot();
            _fireCooldown = 1f / _fireRate;
        }
    }

    public void Shoot()
    {
        ShooteableObject bullet = TurretBulletFactory.Instance.Create(_currentBulletType, _turretMuzzleTransform.position, _turretMuzzleTransform.rotation);
        bullet.Shoot(_turretMuzzleTransform.up);

        #if UNITY_EDITOR
        Debug.Log($"PlayerShoot: Shot a bullet of type {_currentBulletType} from position {_turretMuzzleTransform.position} in direction {_turretMuzzleTransform.forward}");
        #endif
    }
}
