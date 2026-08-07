using UnityEngine;

public class RunningBehaviour : IBehaviours
{
    private bool _active, _targetFound = false;
    private Transform _transform;
    private ChibiSoldierCaptureTarget _closestTarget;
    private FlockingSteering _flocking;
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
        var targetDic = GameManager.Instance.levelManager.ChibiSoldierTargets;
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

        if(distance <= GameManager.Instance.levelManager.TargetCaptureDistance)
        {
            hasReachedTarget = true;
        }
    }

    private Vector3 VectorMinusVector(Vector3 pos1, Vector3 pos2) => (pos1 - pos2);

    public void Reset()
    {
        _active = false;
        _targetFound = false;
        _closestTarget = null;
        hasReachedTarget = false;
    }
}