using UnityEngine;

public class ShootAtPlayerBehaviour : IBehaviours
{
    private Transform[] _damageablePoints;
    private Transform _target = null, _gunMuzzleTransform, _transform;
    private bool _active;
    private readonly float _initialShootInterval;
    private float _shootInterval;
    private BulletType _currentBulletType = BulletType.CommonChibiSoldierBullet;
    private FlockingSteering _flocking;

    public ShootAtPlayerBehaviour(float initialShootInterval, Transform gunMuzzleTransform,
                                Transform t, LayerMask neighborLayerMask, float neighborDetectionRadius, 
                                float speed, float rotationSpeed)
    {
        _initialShootInterval = initialShootInterval;
        _shootInterval = initialShootInterval;
        _gunMuzzleTransform = gunMuzzleTransform;
        _transform = t;
        _flocking = new FlockingSteering(t, neighborLayerMask, neighborDetectionRadius, speed, rotationSpeed);
    }

    public void Active(bool value) {_active = value;}

    public ShootAtPlayerBehaviour GetDamageablePoints(Transform[] damageablePoints)
    {
        _damageablePoints = damageablePoints;
        return this;
    }

    public void ArtificialUpdate()
    {
        if(!_active) return;

        if(_target == null) ChooseTarget();

        Vector3 aimDirection = (_target.position - _transform.position).normalized;
        Vector3 moveDirection = _flocking.CalculateFlockingForce().normalized;
        _flocking.RotateTowards(aimDirection);
        _flocking.Move(moveDirection);
        ShootToTarget();
    }

    private void ChooseTarget()
    {
        if(_target != null || _damageablePoints == null) return;

        int randomIndex = Random.Range(0, _damageablePoints.Length);
        _target = _damageablePoints[randomIndex];
    }

    private void ShootToTarget()
    {
        if(_target == null) return;

        if(_shootInterval > 0)
        {
            _shootInterval -= Time.deltaTime;
            return;
        }

        ShooteableObject bullet = TurretBulletFactory.Instance.Create(_currentBulletType, _gunMuzzleTransform.position, _gunMuzzleTransform.rotation);
        bullet.Shoot(_gunMuzzleTransform.forward);
        _shootInterval = _initialShootInterval;
    }
}