using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private PlayerSettingsSO _playerSettings;
    [SerializeField] private Transform _cannonMuzzleTransform, _turretMuzzleTransform;
    [SerializeField] private Transform _tankHeadTransform;
    [SerializeField] private LayerMask _crosshairRaycastMask = ~0, _cameraRaycastMask = ~0;

    #region Model
    public PlayerMovement playerMovement;
    public PlayerShoot playerShoot;
    public PlayerAim playerAim;
    public PlayerTurretShoot playerTurretShoot;
    public PlayerTurretAim playerTurretAim;
    #endregion

    #region Initialization
    private void Awake()
    {
        playerMovement = new PlayerMovement(transform ,
                                            _playerSettings.rotationSpeed,
                                            _playerSettings.movementSpeed);
        playerShoot = new PlayerShoot(_cannonMuzzleTransform);
        playerAim = new PlayerAim(_tankHeadTransform, 
                                _playerSettings.aimRotationSpeed, 
                                Camera.main, _cannonMuzzleTransform, 
                                _crosshairRaycastMask,
                                _cameraRaycastMask,
                                _playerSettings.minTankHeadPitch,
                                _playerSettings.maxTankHeadPitch);
        playerTurretShoot = new PlayerTurretShoot(_turretMuzzleTransform, 
                                                _playerSettings.turretFireRate, 
                                                _playerSettings.turretFireCooldown);

        if(_playerSettings.tankTurretFollowsCamera)
            playerTurretAim = new PlayerTurretAim();  
    }

    private void Start()
    {
        var inputReader = GetComponent<InputReader>();
        playerMovement.Initialize(inputReader);
        playerShoot.Initialize(inputReader);
        playerTurretShoot.Initialize(inputReader);
        //playerTurretAim?.Initialize(inputReader);
    }
    #endregion

    private void Update()
    {
        playerMovement.ArtificialUpdate();
        playerAim.ArtificialUpdate();
        playerTurretShoot.ArtificialUpdate();
    }

    private void OnDrawGizmos()
    {
        if(_cannonMuzzleTransform == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawRay(_cannonMuzzleTransform.position, _cannonMuzzleTransform.up * 20f);

        if(_turretMuzzleTransform == null) return;

        Gizmos.color = Color.orange;
        Gizmos.DrawRay(_turretMuzzleTransform.position, _turretMuzzleTransform.up * 20f);
    }
}