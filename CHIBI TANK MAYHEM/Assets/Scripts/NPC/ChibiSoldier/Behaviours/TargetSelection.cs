using UnityEngine;

public static class TargetSelection
{
    public static Transform target = null;
    private static Transform[] _damageablePoints;

    public static void Initialize(Transform[] damageablePoints)
    {
        _damageablePoints = damageablePoints;
        ClearTarget();
    }

    public static void ChooseTarget()
    {
        if(target != null || _damageablePoints == null) return;

        int randomIndex = Random.Range(0, _damageablePoints.Length);
        target = _damageablePoints[randomIndex];
    }

    public static void ClearTarget() => target = null;
}
