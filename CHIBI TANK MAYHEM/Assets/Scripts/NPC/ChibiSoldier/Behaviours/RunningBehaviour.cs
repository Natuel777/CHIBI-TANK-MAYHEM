using UnityEngine;

public class RunningBehaviour : IBehaviours
{
    private bool _active, _targetFound = false;
    private Transform _transform, _closestTarget;
    private float _speed, _rotationSpeed, _neighborDetectionRadius;
    private LayerMask _neighborLayerMask;
    //private int _neighborCount = 0;
    private Vector3 _separationForce;
    public bool hasReachedTarget = false;

    public RunningBehaviour(Transform t, float speed, float rotationSpeed, LayerMask neighborLayerMask, float neighborDetectionRadius)
    {
        _transform = t;
        _neighborLayerMask = neighborLayerMask;
        _speed = speed;
        _rotationSpeed = rotationSpeed;
        _neighborDetectionRadius = neighborDetectionRadius;
    }

    public void Active(bool value) {_active = value;}

    public void ArtificialUpdate()
    {
        if(!_active) return;

        if(!_targetFound) FindClosestTarget();

        #region Flocking Loop
        _separationForce = Vector3.zero;
        Collider[] neighborgs = GetNeighbors();
        //_neighborCount = neighborgs.Length;

        if(neighborgs.Length > 0)
        {
            CalculateSeparationForce(neighborgs);
            ApplyAlignment(neighborgs);
            ApplyCohesion(neighborgs);
        }
        #endregion

        if(_closestTarget != null)
        {
            Vector3 direction = VectorMinusVector(_closestTarget.position, _transform.position).normalized;
            Vector3 combinedDirection = (direction + _separationForce).normalized;
            RotateToTarget(direction);
            MoveToTarget(combinedDirection);
            CheckDistanceToTarget();
        }
    }

    private void FindClosestTarget()
    {
        var targetDic = GameManager.Instance.levelManager.ChibiSoldierTargets;
        float closestDistance = Mathf.Infinity;
        Transform closestTarget = null;

        foreach(var target in targetDic)
        {
            if(target.Value) continue;      

            float sqrDistance = VectorMinusVector(target.Key.position, _transform.position).sqrMagnitude;

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

    private void MoveToTarget(Vector3 direction)
    {
        direction.y = 0f;
        _transform.position += direction * _speed * Time.deltaTime;
    }

    private void RotateToTarget(Vector3 direction)
    {
        if(direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            _transform.rotation = Quaternion.Slerp(_transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
        }
    }

    private void CheckDistanceToTarget()
    {
        float distance = VectorMinusVector(_closestTarget.position, _transform.position).magnitude;

        if(distance <= GameManager.Instance.levelManager.TargetCaptureDistance)
        {
            GameManager.Instance.levelManager.UpdateTargetStatus(_closestTarget, true);
            _targetFound = false;
            _closestTarget = null;
            hasReachedTarget = true;
        }
    }

    private Collider[] GetNeighbors()
    {
        return Physics.OverlapSphere(_transform.position, _neighborDetectionRadius, _neighborLayerMask);
    }

    private void CalculateSeparationForce(Collider[] neighborgs)
    {
        foreach(Collider n in neighborgs)
        {
            Vector3 direction = VectorMinusVector(n.transform.position, _transform.position);
            float distance = direction.magnitude;
            Vector3 away = -direction.normalized;

            if(distance > 0)
                _separationForce += away / distance;
        }
    }

    private void ApplyAlignment(Collider[] neighborgs)
    {
        Vector3 neighborForward = Vector3.zero;

        foreach(Collider n in neighborgs)
            neighborForward += n.transform.forward;

        if(neighborForward != Vector3.zero)
            neighborForward.Normalize();

        _separationForce += neighborForward;
    }

    private void ApplyCohesion(Collider[] neighborgs)
    {
        Vector3 neighborCenter = Vector3.zero;

        foreach(Collider n in neighborgs)
            neighborCenter += n.transform.position;
        
        neighborCenter /= neighborgs.Length;
        Vector3 cohesionDirection = VectorMinusVector(neighborCenter, _transform.position).normalized;
        _separationForce += cohesionDirection;
    }

    private Vector3 VectorMinusVector(Vector3 pos1, Vector3 pos2) => (pos1 - pos2);
}
