using UnityEngine;

public abstract class ChibiSoldier : MonoBehaviour
{
    [SerializeField] protected NPCSettingsSO settings;
    [SerializeField] protected LayerMask neighborLayerMask;
    [SerializeField] protected Transform gunMuzzleTransform;
    protected FiniteSateMachine stateMachine;

    #region States
    public IdleState idleState;
    public RunningState runningState;
    public CapturingState capturingState;
    public ShootAtPlayerBehaviour shootAtPlayerBehaviour;
    #endregion

    #region Behaviours
    public IdleBehaviour idleBehaviour;
    public RunningBehaviour runningBehaviour;
    public ParticularHealthModel healthModel;
    #endregion

    public virtual void SendEvent(NPCEvents evt, object data = null) {stateMachine.SendEvent(evt, data);}

    public virtual void SetState(IState state) {stateMachine.SetState(state);}
}
