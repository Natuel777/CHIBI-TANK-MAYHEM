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
    private Dictionary<ChibiSoldierCaptureTarget, List<ChibiSoldier>> _chibiSoldiersPerTarget = new Dictionary<ChibiSoldierCaptureTarget, List<ChibiSoldier>>();
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
            _chibiSoldiersPerTarget.Add(target, new List<ChibiSoldier>());
            _captureProgress.Add(target, 0f);
        }
    }

    public void ArtificialUpdate()
    {
        foreach(var csQTY in _chibiSoldiersPerTarget)
        {
            if(csQTY.Value.Count > 0)
                CaptureTarget(csQTY.Key, csQTY.Value.Count);
        }
    }

    #region Chibi Soldier Target Methods
    public void UpdateTargetStatus(ChibiSoldierCaptureTarget target, bool isCaptured)
    {
        if(_capturedTargets.ContainsKey(target))
        {
            _capturedTargets[target] = isCaptured;

            if(_chibiSoldiersPerTarget.TryGetValue(target, out List<ChibiSoldier> soldiers))
            {
                //Copia: cada soldado, al recibir el evento, sale de CapturingState y eso llama a
                //RemoveChibiSoldierFromCapturedList(), que saca ese mismo soldado de "soldiers" — si
                //se recorriera la lista original, se modificaría en pleno foreach y tira
                //InvalidOperationException. Iterando sobre una copia, esas remociones no afectan el recorrido.
                foreach(ChibiSoldier soldier in soldiers.ToArray())
                    soldier.SendEvent(NPCEvents.ChibiSoldierHasCapturedTarget);
            }
        }
            
    }

    public void AddChibiSoldierToCapturedList(ChibiSoldierCaptureTarget target, ChibiSoldier soldier)
    {
        if(_chibiSoldiersPerTarget.TryGetValue(target, out List<ChibiSoldier> soldiers))
            soldiers.Add(soldier);
    }

    public void RemoveChibiSoldierFromCapturedList(ChibiSoldierCaptureTarget target, ChibiSoldier soldier)
    {
        if(_chibiSoldiersPerTarget.TryGetValue(target, out List<ChibiSoldier> soldiers))
            soldiers.Remove(soldier);
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
