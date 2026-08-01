using UnityEngine;

public class IdleState : IState
{
    private ChibiSoldier _parent;
    
    public IdleState(ChibiSoldier soldier)
    {
        _parent = soldier;
    }

    public void Enter() {_parent.idleBehaviour.Active(true);}
	public void Update() {_parent.idleBehaviour.ArtificialUpdate();}
	public void Exit() {_parent.idleBehaviour.Active(false);}
	public void HandleEvent(NPCEvents evt, object data) {}
}
