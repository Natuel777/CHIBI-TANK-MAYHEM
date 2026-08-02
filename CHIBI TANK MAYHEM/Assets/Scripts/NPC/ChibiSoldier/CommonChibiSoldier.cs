using UnityEngine;

public class CommonChibiSoldier : ChibiSoldier
{
    #region Initialization
    private void Awake()
    {
        stateMachine = new FiniteSateMachine();

        //State Initialization
        idleState = new IdleState(this);
        runningState = new RunningState(this);

        //Behaviour Initialization
        idleBehaviour = new IdleBehaviour(StartCoroutine, settings.idleSearchTimer);
        runningBehaviour = new RunningBehaviour(transform, settings.moveSpeed, 
                                                        settings.rotationSpeed,
                                                        neighborLayerMask, 
                                                        settings.neighborDetectionRadius);
    }

    private void Start()
    {
        SetState(idleState);
    }
    #endregion

    private void Update()
    {
        stateMachine.UpdateState();
    }
}
