using UnityEngine;

public class PlayerTurretAim : IWeaponAimProvider
{
    private readonly Transform _turretTransform, _turretMuzzleTransform;
    private readonly float _turretRotationSpeed, _minTurretPitch, _maxTurretPitch;
    private readonly int _crosshairRaycastMask;
    private Vector3 _crosshairPoint;
    private const float _MaxDistance = 1000f;
    private const float _FallbackDistance = 50f;

    public PlayerTurretAim(Transform turretTransform, Transform turretMuzzleTransform,
                                                    float turretRotationSpeed,
                                                    float turretMinPitch,
                                                    float turretMaxPitch,
                                                    LayerMask crosshairRaycastMask)
    {
        _turretTransform = turretTransform;
        _turretMuzzleTransform = turretMuzzleTransform;
        _turretRotationSpeed = turretRotationSpeed;
        _minTurretPitch = turretMinPitch;
        _maxTurretPitch = turretMaxPitch;
        _crosshairRaycastMask = crosshairRaycastMask;
    }

    public void ArtificialUpdate(Vector3 aimTargetPoint)
    {
        #region Rotar la torreta hacia el punto de apuntado (mismo punto que usa la cabeza del tanque)
        Vector3 direction = aimTargetPoint - _turretTransform.position;

        if(direction.sqrMagnitude > 0.001f)
        {
            direction = AimMath.ClampPitch(direction, _minTurretPitch, _maxTurretPitch);

            Quaternion targetRotation = Quaternion.LookRotation(direction);
            _turretTransform.rotation = Quaternion.RotateTowards(
                _turretTransform.rotation,
                targetRotation,
                _turretRotationSpeed * Time.deltaTime);
        }
        #endregion

        #region Raycast from turret muzzle to get crosshair point
        Ray muzzleRay = new Ray(_turretMuzzleTransform.position, _turretMuzzleTransform.up);

        if(Physics.Raycast(muzzleRay, out RaycastHit crosshairHit, _MaxDistance, _crosshairRaycastMask))
            _crosshairPoint = crosshairHit.point;
        else
            _crosshairPoint = muzzleRay.origin + muzzleRay.direction * _FallbackDistance;
        #endregion
    }

    public Vector3 GetCrosshairPoint() { return _crosshairPoint; }
}
