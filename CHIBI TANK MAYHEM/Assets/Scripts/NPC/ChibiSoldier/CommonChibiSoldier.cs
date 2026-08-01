using UnityEngine;

public class CommonChibiSoldier : ChibiSoldier
{
    private FiniteSateMachine _stateMachine;

    private void Awake()
    {
        _stateMachine = new FiniteSateMachine();

        //State Initialization
        idleState = new IdleState(this);
        runningState = new RunningState(this);

        //Behaviour Initialization
        idleBehaviour = new IdleBehaviour(StartCoroutine, settings.idleSearchTimer);
        runningBehaviour = new RunningBehaviour();
    }

    private void Start()
    {
        SetState(idleState);
    }

    private void Update()
    {
        _stateMachine.UpdateState();
    }

    private void SetState(IState state)
	{
        _stateMachine.SetState(state);
    }

	public void SendEvent(NPCEvents evt, object data = null) {_stateMachine.SendEvent(evt, data);}

}
