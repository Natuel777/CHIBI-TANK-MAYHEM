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
    
	public void Update() {_parent.runningBehaviour.ArtificialUpdate();}
	public void Exit() {_parent.runningBehaviour.Active(false);}
	public void HandleEvent(NPCEvents evt, object data)
    {
        
    }
}
