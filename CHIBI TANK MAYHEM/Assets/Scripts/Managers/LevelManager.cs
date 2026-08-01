using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class LevelManager
{
    [SerializeField] private Transform[] _chibiSoldierTargets;

    private Dictionary<Transform, bool> _capturedTargets = new Dictionary<Transform, bool>();

    #region Getters
    public Dictionary<Transform, bool> ChibiSoldierTargets => _capturedTargets;
    #endregion

    public void Initialize()
    {
        foreach(Transform target in _chibiSoldierTargets)
            _capturedTargets.Add(target, false);
    }

    public void UpdateTargetStatus(Transform target, bool isCaptured)
    {
        if(_capturedTargets.ContainsKey(target))
            _capturedTargets[target] = isCaptured;
    }
}
