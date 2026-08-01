public class FiniteSateMachine
{
    private IState _currentState;

	public void SetState(IState newState)
    	{
        	_currentState?.Exit();
        	_currentState = newState;
        	_currentState?.Enter();
    	}

	public void UpdateState()
	{
		_currentState.Update();
	}

	public void SendEvent(NPCEvents evt, object data = null) {_currentState?.HandleEvent(evt, data);}
}
