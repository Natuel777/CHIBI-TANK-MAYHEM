using UnityEngine;

public class ChibiSoldierCaptureTargetIdleState : IState
{
    private ChibiSoldierCaptureTarget _parent;

    public ChibiSoldierCaptureTargetIdleState(ChibiSoldierCaptureTarget parent)
    {
        _parent = parent;
    }

    public void Enter()
    {
        _parent.view.ShowIdleMesh();
    }

	public void Update() {}

	public void Exit() {}

	public void HandleEvent(NPCEvents evt, object data)
    {
        if(evt == NPCEvents.OnChibiSoldierTargetCaptured)
            _parent.SetState(_parent.cannonState);
    }
}
