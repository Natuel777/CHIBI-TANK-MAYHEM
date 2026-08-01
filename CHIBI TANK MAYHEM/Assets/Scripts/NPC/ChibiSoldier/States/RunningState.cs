using UnityEngine;

public class RunningState : IState
{
    private ChibiSoldier _parent;

    public RunningState(ChibiSoldier soldier)
    {
        _parent = soldier;
    }

    public void Enter() 
    {
        _parent.runningBehaviour.Active(true);
    }
    
	public void Update() 
    {
        _parent.runningBehaviour.ArtificialUpdate();

        if(_parent.runningBehaviour.hasReachedTarget)
        {
            _parent.SendEvent(NPCEvents.ChibiSoldierHasReachedTarget);
            _parent.runningBehaviour.hasReachedTarget = false;
        }
    }
	public void Exit() {_parent.runningBehaviour.Active(false);}
	public void HandleEvent(NPCEvents evt, object data)
    {
        //if(evt == NPCEvents.ChibiSoldierHasReachedTarget)
            //_parent.SetState(_parent.idleState);
    }
}
