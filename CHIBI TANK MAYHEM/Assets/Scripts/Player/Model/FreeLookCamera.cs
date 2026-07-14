using UnityEngine;

public class FreeLookCamera
{
    private readonly Transform _cameraTransform;
    private readonly Transform _orbitTarget;
    private readonly float _sensitivityX;
    private readonly float _sensitivityY;
    private readonly float _minPitch;
    private readonly float _maxPitch;
    private readonly float _distance;
    private readonly float _pivotHeightOffset;
    private float _yaw;
    private float _pitch;
    private Vector2 _lookInput;

    public FreeLookCamera(Transform cameraTransform, Transform orbitTarget,
                        float sensitivityX, float sensitivityY,
                        float minPitch, float maxPitch,
                        float distance, float pivotHeightOffset)
    {
        _cameraTransform = cameraTransform;
        _orbitTarget = orbitTarget;
        _sensitivityX = sensitivityX;
        _sensitivityY = sensitivityY;
        _minPitch = minPitch;
        _maxPitch = maxPitch;
        _distance = distance;
        _pivotHeightOffset = pivotHeightOffset;
        _yaw = orbitTarget.eulerAngles.y;
        _pitch = 15f;
    }

    public void Initialize(InputReader inputReader)
    {
        inputReader.Aim += SetLookInput;
    }

    private void SetLookInput(Vector2 delta)
    {
        _lookInput = delta;
    }

    public void ArtificialUpdate()
    {
        //Actualizamos la rotación objetivo en el eje Y (yaw)
        _yaw += _lookInput.x * _sensitivityX;
        //Actualizamos la rotación objetivo en el eje X (pitch) y la limitamos entre minPitch y maxPitch
        _pitch -= _lookInput.y * _sensitivityY;
        _pitch = Mathf.Clamp(_pitch, _minPitch, _maxPitch);
        //Calculamos la rotación de la cámara en quaterniones a partir de los ángulos de yaw y pitch
        Quaternion orbitRotation = Quaternion.Euler(_pitch, _yaw, 0f);
        Vector3 pivot = _orbitTarget.position + Vector3.up * _pivotHeightOffset;
        Vector3 desiredPosition = pivot + orbitRotation * new Vector3(0f, 0f, -_distance);
        _cameraTransform.position = desiredPosition;
        _cameraTransform.rotation = orbitRotation;
    }
}
