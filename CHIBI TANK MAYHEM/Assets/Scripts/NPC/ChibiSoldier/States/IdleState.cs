using UnityEngine;

public class IdleState : IState
{
    private ChibiSoldier _parent;

    public IdleState(ChibiSoldier soldier)
    {
        _parent = soldier;
    }

    public void Enter() {_parent.idleBehaviour.Active(true);}
	public void Update() 
    {
        _parent.idleBehaviour.ArtificialUpdate();

        if(_parent.idleBehaviour.searchDone)
            _parent.SendEvent(NPCEvents.ChibiSoldierFoundTarget);
    }

	public void Exit() {_parent.idleBehaviour.Active(false);}

	public void HandleEvent(NPCEvents evt, object data)
    {
        if(evt == NPCEvents.ChibiSoldierFoundTarget)
            _parent.SetState(_parent.runningState);
    }
}
