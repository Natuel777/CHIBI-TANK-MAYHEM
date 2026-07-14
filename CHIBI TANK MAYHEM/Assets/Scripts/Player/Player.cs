using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private PlayerSettingsSO _playerSettings;
    [SerializeField] private Transform _cannonMuzzleTransform;
    [SerializeField] private Transform _tankHeadTransform;
    [SerializeField] private LayerMask _crosshairRaycastMask = ~0;

    #region Model
    public PlayerMovement playerMovement;
    public PlayerShoot playerShoot;
    public PlayerAim playerAim;
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
                                _playerSettings.minTankHeadPitch,
                                _playerSettings.maxTankHeadPitch);
    }

    private void Start()
    {
        var inputReader = GetComponent<InputReader>();
        playerMovement.Initialize(inputReader);
        playerShoot.Initialize(inputReader);
    }
    #endregion

    private void Update()
    {
        playerMovement.ArtificialUpdate();
        playerAim.ArtificialUpdate();
    }

    private void OnDrawGizmos()
    {
        if(_cannonMuzzleTransform == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawRay(_cannonMuzzleTransform.position, _cannonMuzzleTransform.up * 20f);
    }
}