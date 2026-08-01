using UnityEngine;

public abstract class ChibiSoldier : MonoBehaviour
{
    [SerializeField] protected NPCSettingsSO settings;
    protected FiniteSateMachine stateMachine;

    #region States
    public IdleState idleState;
    public RunningState runningState;
    #endregion

    #region Behaviours
    public IdleBehaviour idleBehaviour;
    public RunningBehaviour runningBehaviour;
    #endregion

    public virtual void SendEvent(NPCEvents evt, object data = null) {stateMachine.SendEvent(evt, data);}

    public virtual void SetState(IState state) {stateMachine.SetState(state);}
}
