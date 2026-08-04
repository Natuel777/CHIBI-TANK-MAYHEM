using UnityEngine;

public class CapturingState : IState
{
    private ChibiSoldier _parent;
    private ChibiSoldierCaptureTarget _target;

    public CapturingState(ChibiSoldier soldier)
    {
        _parent = soldier;
    }

    public void SetTarget(ChibiSoldierCaptureTarget target) {_target = target;}

    public void Enter()
    {
        GameManager.Instance.levelManager.AddChibiSoldierToCapturedList(_target);
    }

	public void Update()
    {

    }

	public void Exit()
    {
        GameManager.Instance.levelManager.RemoveChibiSoldierFromCapturedList(_target);
    }

	public void HandleEvent(NPCEvents evt, object data)
    {
    }
}
