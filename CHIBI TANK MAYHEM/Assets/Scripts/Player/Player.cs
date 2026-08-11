using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private PlayerSettingsSO _playerSettings;
    [SerializeField] private Transform _cannonMuzzleTransform, _turretMuzzleTransform;
    [SerializeField] private Transform _tankHeadTransform, _turretTransform;
    [SerializeField] private Transform[] _secondaryTurretMuzzleTransforms;
    [SerializeField] private Transform _meshTransform;

    [Tooltip("Anclajes de la suspensión: 6 empties en el fondo del chasis, sobre las orugas (frente/medio/atrás x izquierda/derecha). Desde cada uno sale un raycast hacia abajo que sostiene esa esquina del tanque.")]
    [SerializeField] private Transform[] _suspensionPoints;

    [SerializeField] private LayerMask _crosshairRaycastMask = ~0, _cameraRaycastMask = ~0;
    [SerializeField] private LayerMask _groundMask = ~0;
    [SerializeField] private Transform[] _tankDamageablePoints;

    #region Model
    public PlayerMovement playerMovement;
    public PlayerShoot playerShoot;
    public PlayerAim playerAim;
    public PlayerTurretAim playerTurretAim;
    public PlayerTurretShoot playerTurretShoot;

    #region Health Models
    public ParticularHealthModel bodyHealth;
    public ParticularHealthModel headHealth;
    public ParticularHealthModel turretHealth;
    public ParticularHealthModel trailLFTHealth;
    public ParticularHealthModel trailRGTHealth;
    #endregion
    #endregion

    #region Getters
    public Transform[] TankDamageablePoints => _tankDamageablePoints;
    #endregion
    #region Initialization
    private void Awake()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        playerMovement = new PlayerMovement(transform,
                                            _meshTransform,
                                            _suspensionPoints,
                                            rb,
                                            _playerSettings.maxSpeed,
                                            _playerSettings.acceleration,
                                            _playerSettings.deceleration,
                                            _playerSettings.maxTurnRate,
                                            _playerSettings.turnAcceleration,
                                            _playerSettings.pitchTiltAmount,
                                            _playerSettings.pitchTiltSmoothTime,
                                            _playerSettings.suspensionRestLength,
                                            _playerSettings.suspensionStrength,
                                            _playerSettings.suspensionDampingRatio,
                                            _playerSettings.groundNormalSmoothing,
                                            _groundMask,
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

        bodyHealth = new ParticularHealthModel(_playerSettings.bodyMaxHealth);
        headHealth = new ParticularHealthModel(_playerSettings.headMaxHealth);
        turretHealth = new ParticularHealthModel(_playerSettings.turretMaxHealth);
        trailLFTHealth = new ParticularHealthModel(_playerSettings.trailLFTMaxHealth);
        trailRGTHealth = new ParticularHealthModel(_playerSettings.trailRGTMaxHealth);
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
        DrawSuspensionGizmo();

        if(_cannonMuzzleTransform == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawRay(_cannonMuzzleTransform.position, _cannonMuzzleTransform.up * 20f);

        if(_turretMuzzleTransform == null) return;

        Gizmos.color = Color.orange;
        Gizmos.DrawRay(_turretMuzzleTransform.position, _turretMuzzleTransform.up * 20f);
    }

    //Dibuja el recorrido de cada resorte de la suspensión, igual que los raycasts reales de
    //ApplySuspension() en PlayerMovement. En Play Mode sirve para ver a qué altura flota el tanque:
    //en reposo el suelo debería quedar más o menos a la mitad de cada línea.
    private void DrawSuspensionGizmo()
    {
        if(_playerSettings == null || _suspensionPoints == null) return;

        Gizmos.color = Color.cyan;

        foreach(Transform point in _suspensionPoints)
        {
            if(point == null) continue;

            Gizmos.DrawRay(point.position, -transform.up * _playerSettings.suspensionRestLength);
            Gizmos.DrawWireSphere(point.position, 0.05f);
        }
    }
    #endif
}