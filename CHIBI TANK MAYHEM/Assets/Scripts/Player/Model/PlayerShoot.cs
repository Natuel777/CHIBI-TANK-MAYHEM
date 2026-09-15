using UnityEngine;

public class PlayerShoot : IShootable, IInputInitialize, IServiceConsumer
{
    private bool _initialized = false;
    private Transform _cannonTransform;
    private BulletType _currentBulletType = BulletType.CommonCannonBullet;
    private CannonBulletFactory _cannonBulletFactory;

    public PlayerShoot(Transform cannonTransform)
    {
        _cannonTransform = cannonTransform;
    }

    public void Initialize(InputReader inputReader)
    {
        inputReader.ShootCannon += Shoot;
        _initialized = true;
    }

    public void Shoot()
    {
        if(!_initialized) return;

        if(!TryResolveService())
        {
            Debug.LogWarning("PlayerShoot: Unable to resolve CannonBulletFactory service. Cannot shoot.");
            return;
        } 

        ShooteableObject bullet = _cannonBulletFactory.Create(_currentBulletType, _cannonTransform.position, _cannonTransform.rotation);
        bullet.Shoot(_cannonTransform.up);

        #if UNITY_EDITOR
        Debug.Log($"PlayerShoot: Shot a bullet of type {_currentBulletType} from position {_cannonTransform.position} in direction {_cannonTransform.forward}");
        #endif
    }

    public bool TryResolveService()
    {
        if(_cannonBulletFactory != null) return true;

        return ServiceLocator.Instance.TryGet(out _cannonBulletFactory);
    }
}
