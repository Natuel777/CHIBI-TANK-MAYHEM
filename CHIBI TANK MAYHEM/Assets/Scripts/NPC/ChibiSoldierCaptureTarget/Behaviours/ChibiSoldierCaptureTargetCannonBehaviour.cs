using UnityEngine;

public sealed class ChibiSoldierCaptureTargetCannonBehaviour : IShootable, IBehaviours, IServiceConsumer
{
    private Transform _yRotationCannonMesh, _xRotationCannonMesh, 
                    _targetTransform, _muzzle;
    private bool _active;
    private CannonBulletFactory _cannonBulletFactory;
    private readonly float _rotationSpeed, _shootCooldown;
    private float _shootTimer; 

    public bool IsActive => _active;

    public ChibiSoldierCaptureTargetCannonBehaviour(Transform yRotationCannonMesh, Transform xRotationCannonMesh,
                                                    Transform muzzle, float rotationSpeed, float shootCooldown)
    {
        _yRotationCannonMesh = yRotationCannonMesh;
        _xRotationCannonMesh = xRotationCannonMesh;
        _muzzle = muzzle;
        _rotationSpeed = rotationSpeed;
        _shootCooldown = shootCooldown;
    }

    public void ArtificialUpdate()
    {
        if(!_active) return;

        if(_shootTimer > 0f) _shootTimer -= Time.deltaTime;

        if(_targetTransform == null) return;

        RotateTowardsTarget();

        if(_shootTimer > 0f) return;

        Shoot();
        _shootTimer = _shootCooldown;
    }

    private void RotateTowardsTarget()
    {
        Vector3 toTarget = Vector3.ProjectOnPlane(_targetTransform.position - _yRotationCannonMesh.position, Vector3.up);
        Vector3 barrelDirection = Vector3.ProjectOnPlane(_muzzle.up, Vector3.up);

        if(toTarget.sqrMagnitude < 0.0001f || barrelDirection.sqrMagnitude < 0.0001f) return;

        float angle = Vector3.SignedAngle(barrelDirection, toTarget, Vector3.up);
        float maxStep = _rotationSpeed * Time.deltaTime;

        _yRotationCannonMesh.Rotate(0f, Mathf.Clamp(angle, -maxStep, maxStep), 0f, Space.World);
    }

    public void Shoot()
    {
        if(_targetTransform == null) return;

        if(!TryResolveService()) return;

        ShooteableObject bullet = _cannonBulletFactory.Create(BulletType.CommonCannonBullet, _muzzle.position, _muzzle.rotation);
        bullet.Shoot(_muzzle.up);
    }

    public void Active(bool active) => _active = active;

    public ChibiSoldierCaptureTargetCannonBehaviour UpdateTarget(Transform targetTransform)
    {
        _targetTransform = targetTransform;
        return this;
    }

    public ChibiSoldierCaptureTargetCannonBehaviour EmptyTarget()
    {
        _targetTransform = null;
        return this;
    }

    public bool TryResolveService()
    {
        if(_cannonBulletFactory != null) return true;

        return ServiceLocator.Instance.TryGet(out _cannonBulletFactory);
    }
}
