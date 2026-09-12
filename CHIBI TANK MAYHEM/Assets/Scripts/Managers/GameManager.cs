using UnityEngine;
using UnityEngine.SceneManagement; 

public enum PlatformType
{
    PC,
    Mobile,
    Console
}

public enum GameMode
{
    SinglePlayer,
    Multiplayer
}

public enum MatchMode
{
    DeathMatch,
    Domination,
    BattleRoyale
}

public class GameManager : MonoBehaviour, IGameManager
{
    public static GameManager Instance { get; private set; }
    #region Getters
    public PlatformType CurrentPlatform => _currentPlatform;
    public GameMode CurrentGameMode => _currentGameMode;
    public LevelManager LevelManager => _levelManager;
    #endregion

    private PlatformType _currentPlatform;
    private GameMode _currentGameMode;
    [SerializeField] private bool _testingMobile;
    [SerializeField] private LevelManager _levelManager;

    private void Awake()
    {
        if(Instance != null && Instance != this)
            Destroy(this.gameObject);
        
        else Instance = this;

        ServiceLocator.Instance.Register<IGameManager>(this);

        #if UNITY_ANDROID || UNITY_IOS 
            _currentPlatform = PlatformType.Mobile;

        #elif UNITY_STANDALONE || UNITY_EDITOR
            _currentPlatform = PlatformType.PC;
        #endif

        _levelManager.Initialize();
    }

    private void Start()
    {
        if(_currentPlatform == PlatformType.PC && !_testingMobile)
        {
            var mobileInput = FindAnyObjectByType<MobileInputUIManager>();

            if(mobileInput.gameObject.activeSelf) mobileInput.gameObject.SetActive(false);
        }

        //Dsp se va a expandir con el main menu
        _currentGameMode = GameMode.SinglePlayer;
    }

    private void Update()
    {
        _levelManager.ArtificialUpdate();
    }

    #if UNITY_EDITOR
    public void RestartCurrentLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    #endif

    public void DestroyObject(GameObject obj)
    {
        Destroy(obj);
        #if UNITY_EDITOR
        Debug.Log("Destroyed object: " + obj.name);
        #endif
    }

    private void OnDestroy() 
    {
        ServiceLocator.Instance.Unregister<IGameManager>(this);
    }
}
