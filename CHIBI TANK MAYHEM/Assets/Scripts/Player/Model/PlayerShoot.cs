using UnityEngine;

public class PlayerShoot : IShootable, IInputInitialize, IServiceConsumer
{
    private bool _initialized = false;
    private Transform _cannonTransform;
    private BulletType _currentBulletType = BulletType.CommonCannonBullet;
    private CannonBulletFactory _cannonBulletFactory;
    private float _timer = 0f;
    private readonly float _shootCooldown;
    private bool _canShoot = true;
    private MobileInputUIManager _mobileInputUIManager;

    public PlayerShoot(Transform cannonTransform, float shootCooldown)
    {
        _cannonTransform = cannonTransform;
        _shootCooldown = shootCooldown;
    }

    public void Initialize(InputReader inputReader)
    {
        inputReader.ShootCannon += Shoot;
        _initialized = true;
    }

    public void ArtificialUpdate()
    {
        if(!_initialized) return;

        if(_timer > 0 && !_canShoot)
        {
            _timer -= Time.deltaTime;

            if(TryResolveMobileInputUIManager())
            {
                if(!_mobileInputUIManager.cannonCooldownImage.gameObject.activeSelf) 
                    _mobileInputUIManager.cannonCooldownImage.gameObject.SetActive(true);

                _mobileInputUIManager.cannonCooldownImage.fillAmount = _timer / _shootCooldown;
            }
                
        }
        
        else if(_timer <= 0 && !_canShoot)
        {
            if(TryResolveMobileInputUIManager())
            {
                if(_mobileInputUIManager.cannonCooldownImage.gameObject.activeSelf) 
                    _mobileInputUIManager.cannonCooldownImage.gameObject.SetActive(false);
            }

            _canShoot = true;
        } 
    }

    public void Shoot()
    {
        if(!_initialized || !_canShoot) return;

        if(!TryResolveService())
        {
            Debug.LogWarning("PlayerShoot: Unable to resolve CannonBulletFactory service. Cannot shoot.");
            return;
        } 

        ShooteableObject bullet = _cannonBulletFactory.Create(_currentBulletType, _cannonTransform.position, _cannonTransform.rotation);
        bullet.Shoot(_cannonTransform.up);
        _canShoot = false;
        _timer = _shootCooldown;
    }

    public bool TryResolveService()
    {
        if(_cannonBulletFactory != null) return true;

        return ServiceLocator.Instance.TryGet(out _cannonBulletFactory);
    }

    private bool TryResolveMobileInputUIManager()
    {
        if(_mobileInputUIManager != null) return true;

        return ServiceLocator.Instance.TryGet(out _mobileInputUIManager);
    }
}
