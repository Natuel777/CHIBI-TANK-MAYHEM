using UnityEngine;

public abstract class ChibiSoldier : MonoBehaviour
{
    [SerializeField] protected NPCSettingsSO settings;

    #region States
    protected IdleState idleState;
    protected RunningState runningState;
    #endregion

    #region Behaviours
    public IdleBehaviour idleBehaviour;
    public RunningBehaviour runningBehaviour;
    #endregion
}
