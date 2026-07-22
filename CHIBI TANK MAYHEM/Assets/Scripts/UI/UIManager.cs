using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    #region Model
    public CrosshairUI crosshairUI;
    public CrosshairUI turretCrosshairUI;
    #endregion

    private Camera _camera;
    [SerializeField] private RectTransform _crosshair;
    [SerializeField] private RectTransform _turretCrosshair;
    [SerializeField] private PlayerSettingsSO _playerSettings;

    [Header("UI Components")]
    [SerializeField] private Image _turretCrosshairImage;

    private void Awake()
    {
        _camera = Camera.main;
        crosshairUI = new CrosshairUI(_camera, _crosshair);

        if(_turretCrosshair != null && _playerSettings.tankTurretFollowsCamera) 
            turretCrosshairUI = new CrosshairUI(_camera, _turretCrosshair);

        else _turretCrosshairImage.enabled = false;
    }

    private void Start()
    {
        var player = FindAnyObjectByType<Player>();
        crosshairUI.Initialize(player, player.playerAim);

        if(turretCrosshairUI != null && player.playerTurretAim != null)
            turretCrosshairUI.Initialize(player, player.playerTurretAim);
    }

    private void LateUpdate()
    {
        crosshairUI.ArtificialLateUpdate();
        turretCrosshairUI?.ArtificialLateUpdate();
    }
}
