public class FiniteSateMachine
{
    public IState currentState;

	public void SetState(IState newState)
    {
    	currentState?.Exit();
    	currentState = newState;
    	currentState?.Enter();
		PlayerEvents.OnAnyStateChanged.Invoke();
    }

	public void UpdateState() => currentState.Update();

	public void SendEvent(NPCEvents evt, object data = null) {currentState?.HandleEvent(evt, data);}
}
