using UnityEngine;

public class PlayerCollision : IServiceConsumer
{
    private Rigidbody _rb;
    private float _timer = 0f;
    private float _movementCancelationThreshold;
    private IPlayer _player;

    public PlayerCollision(Rigidbody rb, float movementCancelationThreshold)
    {
        _rb = rb;
        _movementCancelationThreshold = movementCancelationThreshold;

    }

    public void ArticifialCollisionStay()
    {
        if(!TryResolveService()) return;

        if(_player.MovementCanceledOnCollision) return;

        if(_timer >= _movementCancelationThreshold) return;

        _player.SetMovementCanceledOnCollision(true);
        _timer += Time.fixedDeltaTime;
    }

    public void ArtificialCollisionExit()
    {
        if(!TryResolveService()) return;

        if(!_player.MovementCanceledOnCollision) return;

        _player.SetMovementCanceledOnCollision(false);
        _timer = 0f;
    }

    public bool TryResolveService()
    {
        if(_player != null) return true;

        if(!ServiceLocator.Instance.TryGet(out IPlayer playerInterface)) return false;
        
        if(playerInterface is not Player player) return false;

        _player = player;
        return true;
    }
}
