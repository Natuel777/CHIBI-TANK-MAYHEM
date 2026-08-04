using UnityEngine;
using UnityEngine.SceneManagement; 

public enum PlatformType
{
    PC,
    Mobile,
    Console
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public PlatformType CurrentPlatform => _currentPlatform;

    private PlatformType _currentPlatform;
    [SerializeField] private bool _testingMobile;

    public LevelManager levelManager;

    private void Awake()
    {
        if(Instance != null && Instance != this)
            Destroy(this.gameObject);
        
        else Instance = this;

        #if UNITY_ANDROID || UNITY_IOS 
            _currentPlatform = PlatformType.Mobile;

        #elif UNITY_STANDALONE || UNITY_EDITOR
            _currentPlatform = PlatformType.PC;
        #endif

        levelManager.Initialize();
    }

    private void Start()
    {
        if(_currentPlatform == PlatformType.PC && !_testingMobile)
        {
            var mobileInput = FindAnyObjectByType<MobileInputUIManager>();

            if(mobileInput.gameObject.activeSelf) mobileInput.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        levelManager.ArtificialUpdate();
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
}
