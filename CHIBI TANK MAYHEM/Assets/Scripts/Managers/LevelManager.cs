using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class LevelManager
{
    [Header("Chibi Soldier Targets")]
    [SerializeField] private ChibiSoldierCaptureTarget[] _chibiSoldierTargets;
    [SerializeField] private float _targetCaptureDistance = 4f;
    [SerializeField] private float _captureThreshold = 100f;

    private Dictionary<ChibiSoldierCaptureTarget, bool> _capturedTargets = new Dictionary<ChibiSoldierCaptureTarget, bool>();
    private Dictionary<ChibiSoldierCaptureTarget, int> _chibiSoldierCountPerTarget = new Dictionary<ChibiSoldierCaptureTarget, int>();
    private Dictionary<ChibiSoldierCaptureTarget, float> _captureProgress = new Dictionary<ChibiSoldierCaptureTarget, float>();

    #region Getters
    public Dictionary<ChibiSoldierCaptureTarget, bool> ChibiSoldierTargets => _capturedTargets;
    public float TargetCaptureDistance => _targetCaptureDistance;
    #endregion

    public void Initialize()
    {
        foreach(ChibiSoldierCaptureTarget target in _chibiSoldierTargets)
        {
            _capturedTargets.Add(target, false);
            _chibiSoldierCountPerTarget.Add(target, 0);
            _captureProgress.Add(target, 0f);
        }
    }

    public void ArtificialUpdate()
    {
        foreach(var csQTY in _chibiSoldierCountPerTarget)
        {
            if(csQTY.Value > 0)
            {
                CaptureTarget(csQTY.Key, csQTY.Value);
            }
        }
    }

    #region Chibi Soldier Target Methods
    public void UpdateTargetStatus(ChibiSoldierCaptureTarget target, bool isCaptured)
    {
        if(_capturedTargets.ContainsKey(target))
            _capturedTargets[target] = isCaptured;
    }

    public void AddChibiSoldierToCapturedList(ChibiSoldierCaptureTarget target)
    {
        if(_chibiSoldierCountPerTarget.TryGetValue(target, out int count))
            _chibiSoldierCountPerTarget[target] = count + 1;
    }

    public void RemoveChibiSoldierFromCapturedList(ChibiSoldierCaptureTarget target)
    {
        if(_chibiSoldierCountPerTarget.TryGetValue(target, out int count) && count > 0)
            _chibiSoldierCountPerTarget[target] = count - 1;
    }

    private void CaptureTarget(ChibiSoldierCaptureTarget target, int chibiSoldierCount)
    {
        if(_capturedTargets[target]) return;

        float captureSpeed = chibiSoldierCount;
        _captureProgress[target] += captureSpeed * Time.deltaTime;
        target.SetCaptureProgress(_captureProgress[target] / _captureThreshold);

        if(_captureProgress[target] >= _captureThreshold)
            UpdateTargetStatus(target, true);
    }
    #endregion
}
