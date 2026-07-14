using UnityEngine;

public class UIManager : MonoBehaviour
{
    #region Model
    public CrosshairUI crosshairUI;
    #endregion

    private Camera _camera;
    [SerializeField] private RectTransform _crosshair;

    private void Awake()
    {
        _camera = Camera.main;
        crosshairUI = new CrosshairUI(_camera, _crosshair);
    }

    private void Start()
    {
        var player = FindAnyObjectByType<Player>();
        crosshairUI.Initialize(player, player.playerAim);
    }

    private void LateUpdate()
    {
        crosshairUI.ArtificialLateUpdate();
    }
}
