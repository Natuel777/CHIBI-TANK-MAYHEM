using UnityEngine;

public enum ChibiSoldierType
{
    Common,
    Heavy,
}

public abstract class ChibiSoldier : MonoBehaviour, IServiceConsumer
{
    [SerializeField] protected NPCSettingsSO settings;
    [SerializeField] protected LayerMask neighborLayerMask;
    [SerializeField] protected Transform gunMuzzleTransform, shoulderTransform;
    [SerializeField] protected ChibiSoldierType type;
    protected FiniteSateMachine stateMachine;
    protected ChibiSoldierFactory chibiSoldierFactory;

    #region States
    public IdleState idleState;
    public RunningState runningState;
    public CapturingState capturingState;
    #endregion

    #region Behaviours
    public IdleBehaviour idleBehaviour;
    public RunningBehaviour runningBehaviour;
    public ParticularHealthModel healthModel;
    public ArmAim armAim;
    public ShootAtPlayerBehaviour shootAtPlayerBehaviour;
    #endregion

    #region Properties
    public ChibiSoldierType ChibiSoldierType => type;
    #endregion

    public virtual void SendEvent(NPCEvents evt, object data = null) {stateMachine.SendEvent(evt, data);}

    public virtual void SetState(IState state) {stateMachine.SetState(state);}

    public IState GetCurrentState() => stateMachine.currentState;

    public bool TryResolveService()
    {
        if(chibiSoldierFactory != null) return true;

        return ServiceLocator.Instance.TryGet(out chibiSoldierFactory);
    }
}
