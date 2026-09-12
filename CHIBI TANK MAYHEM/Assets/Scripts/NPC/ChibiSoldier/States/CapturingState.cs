using UnityEngine;

public class CapturingState : IState
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
        if(!TryResolveGameManager()) return;

        _gameManager.LevelManager.AddChibiSoldierToCapturedList(_target);
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

        _gameManager.LevelManager.RemoveChibiSoldierFromCapturedList(_target);
    }

	public void HandleEvent(NPCEvents evt, object data) {}

    //Mismo motivo que en RunningBehaviour: resolver el service acá (recién cuando se entra a este
    //estado, mucho después del arranque de la escena) en vez de en el constructor evita depender del
    //orden de Awake entre este NPC y GameManager.
    private bool TryResolveGameManager()
    {
        if(_gameManager != null) return true;
        
        if(!ServiceLocator.Instance.TryGet(out IGameManager gmInterface)) return false;
        
        if(gmInterface is not GameManager gameManager) return false;

        _gameManager = gameManager;
        return true;
    }
}
