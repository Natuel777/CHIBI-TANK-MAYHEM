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
        healthModel = new ChibiSoldierHealthModel(settings.maxHealth);
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

    #if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if(settings == null) return;

        //Visualiza el radio que usa RunningBehaviour.GetNeighbors() (Physics.OverlapSphere) para
        //detectar vecinos cercanos y calcular separación/alineación (flocking).
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, settings.neighborDetectionRadius);
    }
    #endif
}