using UnityEngine;

public class CrosshairUI
{
    private IWeaponAimProvider _provider;
    private Camera _camera;
    private RectTransform _crosshair;
    private Player _player;

    public CrosshairUI(Camera camera, RectTransform crosshair)
    {
        _camera = camera;
        _crosshair = crosshair;
    }

    public void Initialize(Player player, IWeaponAimProvider provider)
    {
        _player = player;
        _provider = provider;
    }

    public void ArtificialLateUpdate()
    {
        //Si nunca se llamó Initialize (ej. la torreta está deshabilitada por PlayerSettingsSO),
        //no hay provider del que leer el punto de apuntado — no hacemos nada ese frame.
        if(_provider == null) return;

        Vector3 worldPoint = _provider.GetCrosshairPoint();
        Vector3 screen = _camera.WorldToScreenPoint(worldPoint);
        //Si el punto está detrás de la cámara, WorldToScreenPoint devuelve una proyección
        //espejada válida en apariencia pero incorrecta (z negativo) — hay que ocultar el crosshair.
        bool isBehindCamera = screen.z < 0f;
        
        if(_crosshair.gameObject.activeSelf == isBehindCamera)
            _crosshair.gameObject.SetActive(!isBehindCamera);

        if(!isBehindCamera) _crosshair.position = screen;

        float sqrDistance = (_player.transform.position - worldPoint).sqrMagnitude;

        switch(sqrDistance)
        {
            case < 100f:
                _crosshair.localScale = Vector3.one * 2f; 
                break;
            case >= 100f and < 900f:
                _crosshair.localScale = Vector3.one * 1.5f;
                break;
            default:
                _crosshair.localScale = Vector3.one * 1f;
                break;
        }
    }
}
