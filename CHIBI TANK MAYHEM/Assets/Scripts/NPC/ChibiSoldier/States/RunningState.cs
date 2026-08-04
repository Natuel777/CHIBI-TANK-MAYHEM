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

        //Se manda el target como data del evento porque Exit() (más abajo) llama a
        //runningBehaviour.Reset(), que limpia ClosestTarget ANTES de que CapturingState.Enter()
        //llegue a leerlo. Si no lo pasamos acá, se pierde la referencia al target a capturar.
        if(_parent.runningBehaviour.hasReachedTarget)
            _parent.SendEvent(NPCEvents.ChibiSoldierCapturingTarget, _parent.runningBehaviour.ClosestTarget);
    }
	public void Exit() {_parent.runningBehaviour.Reset();}

	public void HandleEvent(NPCEvents evt, object data)
    {
        if(evt == NPCEvents.ChibiSoldierCapturingTarget)
        {
            _parent.capturingState.SetTarget((ChibiSoldierCaptureTarget)data);
            _parent.SetState(_parent.capturingState);
        }
    }
}
