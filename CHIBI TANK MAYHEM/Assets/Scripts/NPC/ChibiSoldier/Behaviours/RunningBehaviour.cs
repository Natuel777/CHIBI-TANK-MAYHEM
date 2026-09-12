using UnityEngine;

public class RunningBehaviour : IBehaviours, IServiceConsumer
{
    private bool _active, _targetFound = false;
    private Transform _transform;
    private ChibiSoldierCaptureTarget _closestTarget;
    private FlockingSteering _flocking;
    private IGameManager _gameManager;
    public bool hasReachedTarget = false;
    public ChibiSoldierCaptureTarget ClosestTarget => _closestTarget;

    public RunningBehaviour(Transform t, float speed, float rotationSpeed, LayerMask neighborLayerMask, float neighborDetectionRadius)
    {
        _transform = t;
        _flocking = new FlockingSteering(t, neighborLayerMask, neighborDetectionRadius, speed, rotationSpeed);
    }

    public void Active(bool value) {_active = value;}

    public void ArtificialUpdate()
    {
        if(!_active) return;

        if(!_targetFound) FindClosestTarget();

        if(_closestTarget == null) return;

        Vector3 direction = VectorMinusVector(_closestTarget.transform.position, _transform.position).normalized;
        Vector3 combinedDirection = (direction + _flocking.CalculateFlockingForce()).normalized;

        _flocking.RotateTowards(direction);
        _flocking.Move(combinedDirection);
        CheckDistanceToTarget();
    }

    private void FindClosestTarget()
    {
        if(!TryResolveService()) return;

        var targetDic = _gameManager.LevelManager.ChibiSoldierTargets;
        float closestDistance = Mathf.Infinity;
        ChibiSoldierCaptureTarget closestTarget = null;

        foreach(var target in targetDic)
        {
            if(target.Value) continue;

            float sqrDistance = VectorMinusVector(target.Key.transform.position, _transform.position).sqrMagnitude;

            if(sqrDistance < closestDistance)
            {
                closestDistance = sqrDistance;
                closestTarget = target.Key;
            }
        }

        if(closestTarget != null)
        {
            _closestTarget = closestTarget;
            _targetFound = true;
        }
    }

    private void CheckDistanceToTarget()
    {
        float distance = VectorMinusVector(_closestTarget.transform.position, _transform.position).magnitude;

        if(distance <= _gameManager.LevelManager.TargetCaptureDistance)
            hasReachedTarget = true;
    }

    private Vector3 VectorMinusVector(Vector3 pos1, Vector3 pos2) => (pos1 - pos2);

    public void Reset()
    {
        _active = false;
        _targetFound = false;
        _closestTarget = null;
        hasReachedTarget = false;
    }

    //Lazy en vez de resolverse una sola vez en el constructor: si el Awake() de este NPC corre
    //antes que el de GameManager, ServiceLocator todavía no tiene nada registrado. Reintentando acá
    //(que se llama todos los frames hasta encontrar target) el service termina apareciendo apenas
    //GameManager haga su Awake, sin depender de en qué orden Unity llame a los Awake de cada objeto.
    public bool TryResolveService()
    {
        if(_gameManager != null) return true;

        if(!ServiceLocator.Instance.TryGet(out IGameManager gmInterface)) return false;
        
        if(gmInterface is not GameManager gameManager) return false;

        _gameManager = gameManager;
        return true;
    }
}