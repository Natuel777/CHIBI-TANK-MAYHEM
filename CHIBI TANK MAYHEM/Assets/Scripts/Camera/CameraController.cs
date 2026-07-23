using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private PlayerSettingsSO _playerSettings;
    [SerializeField] private Transform _orbitTarget;
    [SerializeField] private InputReader _inputReader;

    #region Model
    public FreeLookCamera cameraOrbit;
    #endregion

    private void Awake()
    {
        cameraOrbit = new FreeLookCamera(
            transform,
            _orbitTarget,
            _playerSettings.orbitSensitivityX,
            _playerSettings.orbitSensitivityY,
            _playerSettings.orbitMinPitch,
            _playerSettings.orbitMaxPitch,
            _playerSettings.orbitDistance,
            _playerSettings.orbitPivotHeightOffset);
        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;
    }

    private void Start()
    {
        cameraOrbit.Initialize(_inputReader);
    }

    private void Update()
    {
        cameraOrbit.ArtificialUpdate();
    }
}
