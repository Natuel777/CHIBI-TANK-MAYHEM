using UnityEngine;

public class CapturingState : IState, IServiceConsumer
{
    private ChibiSoldier _parent;
    private ChibiSoldierCaptureTarget _target;
    private IGameManager _gameManager;

    public CapturingState(ChibiSoldier soldier)
    {
        _parent = soldier;
    }

    public void SetTarget(ChibiSoldierCaptureTarget target) {_target = target;}

    public void Enter()
    {
        if(!TryResolveService()) return;

        _gameManager.LevelManager.AddChibiSoldierToCapturedList(_target, _parent);
        _parent.shootAtPlayerBehaviour.Active(true);
    }

	public void Update()
    {
        _parent.shootAtPlayerBehaviour.ArtificialUpdate();
        _parent.armAim.ArtificialUpdate();
    }

	public void Exit()
    {
        if(_gameManager == null) return;

        _gameManager.LevelManager.RemoveChibiSoldierFromCapturedList(_target, _parent);
    }

	public void HandleEvent(NPCEvents evt, object data)
    {
        if(evt == NPCEvents.ChibiSoldierHasCapturedTarget)
            _parent.SetState(_parent.runningState);
    }

    public bool TryResolveService()
    {
        if(_gameManager != null) return true;

        if(!ServiceLocator.Instance.TryGet(out IGameManager gmInterface)) return false;
        
        if(gmInterface is not GameManager gameManager) return false;

        _gameManager = gameManager;
        return true;
    }
}
