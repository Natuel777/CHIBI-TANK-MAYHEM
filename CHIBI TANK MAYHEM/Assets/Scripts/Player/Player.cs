using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private PlayerSettingsSO _playerSettings;
    [SerializeField] private Transform _cannonMuzzleTransform;
    [SerializeField] private Transform _tankHeadTransform;

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
        playerAim = new PlayerAim(_tankHeadTransform, _playerSettings.aimRotationSpeed);
    }

    private void Start()
    {
        var inputReader = GetComponent<InputReader>();
        playerMovement.Initialize(inputReader);
        playerShoot.Initialize(inputReader);
        playerAim.Initialize(inputReader);
    }
    #endregion

    private void Update()
    {
        playerMovement.ArtificialUpdate();
        playerAim.ArtificialUpdate();
    }
}
