using UnityEngine;

public sealed class ChibiSoldierCaptureTargetCannonState : IState
{
    private ChibiSoldierCaptureTarget _parent;

    public ChibiSoldierCaptureTargetCannonState(ChibiSoldierCaptureTarget parent)
    {
        _parent = parent;
    }

    public void Enter()
    {
        _parent.view.ShowCannonMesh();
        _parent.cannonBehaviour.Active(true);
    }

	public void Update()
    {
        _parent.cannonBehaviour.ArtificialUpdate();
    }

	public void Exit()
    {
        _parent.cannonBehaviour.Active(false);
    }

	public void HandleEvent(NPCEvents evt, object data)
    {

    }
}
