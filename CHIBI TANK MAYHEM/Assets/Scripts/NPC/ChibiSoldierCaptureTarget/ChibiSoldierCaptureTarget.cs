using UnityEngine;
using UnityEngine.UI;

public class ChibiSoldierCaptureTarget : MonoBehaviour
{
    [SerializeField] private Slider _captureBar;
    [SerializeField] private Transform _yRotationCannonMesh, _xRotationCannonMesh, _muzzle;
    [SerializeField] private GameObject _idleMesh, _cannonMesh;
    [SerializeField] private float _rotationSpeed, _shootCooldown;
    private FiniteStateMachine _stateMachine;

    #region MVC
    public ChibiSoldierCaptureTargetView view;
    public ChibiSoldierCaptureTargetCannonBehaviour cannonBehaviour;
    #endregion

    #region States
    public ChibiSoldierCaptureTargetIdleState idleState;
    public ChibiSoldierCaptureTargetCannonState cannonState;
    #endregion

    #region Initialization
    private void Awake()
    {
        _stateMachine = new FiniteStateMachine();
        
        #region Behaviours Initialization
        view = new ChibiSoldierCaptureTargetView(_captureBar, _idleMesh, _cannonMesh);
        cannonBehaviour = new ChibiSoldierCaptureTargetCannonBehaviour(_yRotationCannonMesh, _xRotationCannonMesh, _muzzle, _rotationSpeed, _shootCooldown);
        #endregion
        
        #region State Initialization
        idleState = new ChibiSoldierCaptureTargetIdleState(this);
        cannonState = new ChibiSoldierCaptureTargetCannonState(this);
        #endregion
    }

    private void Start()
    {
        SetState(idleState);
    }
    #endregion

    private void Update()
    {
        _stateMachine.UpdateState();
    }

    public void SendEvent(NPCEvents evt, object data = null) {_stateMachine.SendEvent(evt, data);}

    public void SetState(IState state) {_stateMachine.SetState(state);}

    public IState GetCurrentState() => _stateMachine.currentState;

    private void OnTriggerEnter(Collider other) 
    {
        if(other.TryGetComponent(out Player player))
        {
            if(!cannonBehaviour.IsActive) return;

            cannonBehaviour.UpdateTarget(player.transform);
        }
    }

    private void OnTriggerExit(Collider other) 
    {
        
        if(other.TryGetComponent(out Player player))
        {
            if(!cannonBehaviour.IsActive) return;

            cannonBehaviour.EmptyTarget();
        }
    }
}
