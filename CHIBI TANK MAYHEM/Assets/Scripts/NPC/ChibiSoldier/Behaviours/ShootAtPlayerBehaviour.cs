using UnityEngine;

public class ShootAtPlayerBehaviour : IBehaviours, IServiceConsumer
{
    private Transform _target = null, _gunMuzzleTransform, _transform;
    private bool _active;
    private readonly float _initialShootInterval;
    private float _shootInterval;
    private const float _flockingDeadzone = 0.05f;
    private BulletType _currentBulletType = BulletType.CommonChibiSoldierBullet;
    private FlockingSteering _flocking;
    private TurretBulletFactory _turretBulletFactory;
    private ArmAim _armAim;

    public ShootAtPlayerBehaviour(float initialShootInterval, Transform gunMuzzleTransform,
                                Transform t, LayerMask neighborLayerMask, float neighborDetectionRadius, 
                                float speed, float rotationSpeed, ArmAim armAim)
    {
        _initialShootInterval = initialShootInterval;
        _shootInterval = initialShootInterval;
        _gunMuzzleTransform = gunMuzzleTransform;
        _transform = t;
        _flocking = new FlockingSteering(t, neighborLayerMask, neighborDetectionRadius, speed, rotationSpeed);
        _armAim = armAim;
    }

    public void Active(bool value) {_active = value;}

    public void ArtificialUpdate()
    {
        if(!_active) return;

        if(_target == null)
        {
            if(TargetSelection.target == null) TargetSelection.ChooseTarget();

            _target = TargetSelection.target;
            _armAim.SetTarget(_target);
            TargetSelection.ClearTarget();
        } 
        
        Vector3 flockingForce = _flocking.CalculateFlockingForce(includeAlignment: false);
        Vector3 aimDirection = (_target.position - _transform.position).normalized;
        _flocking.RotateTowards(aimDirection);

        //La fuerza de separación/cohesión casi nunca da EXACTAMENTE cero aunque los vecinos ya estén
        //bien acomodados, siempre queda un resto mínimo. Antes se usaba .normalized, así que ese resto
        //(por chico que sea) se convertía en un paso a velocidad MÁXIMA cada frame, y como su dirección
        //cambia un poco de frame a frame, el bot vibraba en el lugar. Acá: si la fuerza es menor al
        //umbral, no se mueve; si no, se limita a magnitud 1 (en vez de normalizar) para que el paso sea
        //proporcional a cuánta corrección hace falta, no siempre a fondo.
        if(flockingForce.sqrMagnitude > _flockingDeadzone * _flockingDeadzone)
            _flocking.Move(Vector3.ClampMagnitude(flockingForce, 1f));

        ShootToTarget();
    }

    private void ShootToTarget()
    {
        if(_target == null) return;

        if(!TryResolveService()) return;

        if(_shootInterval > 0)
        {
            _shootInterval -= Time.deltaTime;
            return;
        }

        ShooteableObject bullet = _turretBulletFactory.Create(_currentBulletType, 
                                                                    _gunMuzzleTransform.position, 
                                                                    _gunMuzzleTransform.rotation);
        bullet.Shoot(_gunMuzzleTransform.forward);
        _shootInterval = _initialShootInterval;
    }

    public bool TryResolveService()
    {
        if(_turretBulletFactory != null) return true;

        return ServiceLocator.Instance.TryGet(out _turretBulletFactory);
    }
}