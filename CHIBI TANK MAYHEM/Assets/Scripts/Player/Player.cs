using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private PlayerSettingsSO _playerSettings;
    [SerializeField] private Transform _cannonMuzzleTransform, _turretMuzzleTransform;
    [SerializeField] private Transform _tankHeadTransform, _turretTransform;
    [SerializeField] private Transform[] _secondaryTurretMuzzleTransforms;
    [SerializeField] private Transform _meshTransform;
    [SerializeField] private LayerMask _crosshairRaycastMask = ~0, _cameraRaycastMask = ~0;

    #region Model
    public PlayerMovement playerMovement;
    public PlayerShoot playerShoot;
    public PlayerAim playerAim;
    public PlayerTurretAim playerTurretAim;
    public PlayerTurretShoot playerTurretShoot;
    #endregion

    #region Initialization
    private void Awake()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        playerMovement = new PlayerMovement(transform,
                                            _meshTransform,
                                            rb,
                                            _playerSettings.maxSpeed,
                                            _playerSettings.acceleration,
                                            _playerSettings.deceleration,
                                            _playerSettings.maxTurnRate,
                                            _playerSettings.turnAcceleration,
                                            _playerSettings.pitchTiltAmount,
                                            _playerSettings.pitchTiltSmoothTime,
                                            _playerSettings.pitchTiltDeadzone,
                                            _playerSettings.centerOfMassOffset);
        playerShoot = new PlayerShoot(_cannonMuzzleTransform);
        playerAim = new PlayerAim(_tankHeadTransform,
                                _playerSettings.aimRotationSpeed,
                                Camera.main, _cannonMuzzleTransform,
                                _crosshairRaycastMask,
                                _cameraRaycastMask,
                                _playerSettings.minTankHeadPitch,
                                _playerSettings.maxTankHeadPitch);
        playerTurretShoot = new PlayerTurretShoot(_turretMuzzleTransform,
                                                transform,
                                                _playerSettings.turretFireRate,
                                                _playerSettings.turretFireCooldown,
                                                _playerSettings.secondaryTurretFireRate,
                                                _playerSettings.secondaryTurretFireCooldown,
                                                _playerSettings.minSecondaryTurretPitch,
                                                _playerSettings.maxSecondaryTurretPitch,
                                                _playerSettings.secondaryTurretsCanAim,
                                                _playerSettings.minSecondaryTurretYaw,
                                                _playerSettings.maxSecondaryTurretYaw);

        if(_playerSettings.tankTurretFollowsCamera)
            playerTurretAim = new PlayerTurretAim(_turretTransform,
                                                _turretMuzzleTransform,
                                                _playerSettings.turretRotationSpeed,
                                                _playerSettings.minTurretPitch,
                                                _playerSettings.maxTurretPitch,
                                                _crosshairRaycastMask);
    }

    private void Start()
    {
        var inputReader = GetComponent<InputReader>();
        playerMovement.Initialize(inputReader);
        playerShoot.Initialize(inputReader);
        playerTurretShoot.Initialize(inputReader);

        if(_secondaryTurretMuzzleTransforms.Length > 0)
            playerTurretShoot.SetSecondaryMuzzleTransforms(_secondaryTurretMuzzleTransforms);
    }
    #endregion

    private void Update()
    {
        playerAim.ArtificialUpdate();
        playerTurretAim?.ArtificialUpdate(playerAim.AimTargetPoint);
        playerTurretShoot.ArtificialUpdate(playerAim.AimTargetPoint);
    }

    private void FixedUpdate()
    {
        playerMovement.ArtificialFixedUpdate();
    }

    #if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if(_cannonMuzzleTransform == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawRay(_cannonMuzzleTransform.position, _cannonMuzzleTransform.up * 20f);

        if(_turretMuzzleTransform == null) return;

        Gizmos.color = Color.orange;
        Gizmos.DrawRay(_turretMuzzleTransform.position, _turretMuzzleTransform.up * 20f);
    }
    #endif
}