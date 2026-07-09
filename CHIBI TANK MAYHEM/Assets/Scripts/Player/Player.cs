using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private PlayerSettingsSO playerSettings;
    [SerializeField] private Transform cannonMuzzleTransform;

    #region Model
    public PlayerMovement playerMovement;
    public PlayerShoot playerShoot;
    #endregion

    #region Initialization
    private void Awake()
    {
        playerMovement = new PlayerMovement(transform ,
                                            playerSettings.rotationSpeed,
                                            playerSettings.movementSpeed);
        playerShoot = new PlayerShoot(cannonMuzzleTransform);
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
    }
}
