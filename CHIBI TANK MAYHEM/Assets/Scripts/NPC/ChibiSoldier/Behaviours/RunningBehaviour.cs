using UnityEngine;

public class RunningBehaviour : IBehaviours
{
    private bool _active, _targetFound;
    private Transform _transform, _closestTarget;

    public RunningBehaviour(Transform t)
    {
        _transform = t;
    }

    public void Active(bool value) {_active = value;}

    public void ArtificialUpdate()
    {
        if(!_active) return;

        if(!_targetFound) FindClosestTarget();

        if(_closestTarget != null) MoveToTarget();
    }

    private void FindClosestTarget()
    {
        var targetDic = GameManager.Instance.levelManager.ChibiSoldierTargets;
        float closestDistance = Mathf.Infinity;
        Transform closestTarget = null;

        foreach(var target in targetDic)
        {
            if(target.Value)
            {
                targetDic.Remove(target.Key);
                continue;
            }

            float sqrDistance = (target.Key.position - _transform.position).sqrMagnitude;

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

    private void MoveToTarget(){}
}
