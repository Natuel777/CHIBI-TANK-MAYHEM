using UnityEngine;

public class UIManager : MonoBehaviour
{
    #region Model
    public CrosshairUI crosshairUI;
    public CrosshairUI turretCrosshairUI;
    #endregion

    private Camera _camera;
    [SerializeField] private RectTransform _crosshair;
    [SerializeField] private RectTransform _turretCrosshair;

    private void Awake()
    {
        _camera = Camera.main;
        crosshairUI = new CrosshairUI(_camera, _crosshair);

        if(_turretCrosshair != null)
            turretCrosshairUI = new CrosshairUI(_camera, _turretCrosshair);
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
