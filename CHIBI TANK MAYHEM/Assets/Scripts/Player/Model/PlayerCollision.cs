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

    public void ArticifialCollisionStay(Collision other)
    {
        if(!TryResolveService()) return;

        if(_player.MovementCanceledOnCollisionForward || _player.MovementCanceledOnCollisionBack) return;

        if(_timer >= _movementCancelationThreshold) return;

        Vector3 contactPoint = other.GetContact(0).point;
        Vector3 direction = (contactPoint - _player.transform.position).normalized;
        float forward = Vector3.Dot(_player.transform.forward, direction);

        if(forward > 0.5f)
        {
            _player.SetMovementCanceledOnCollision(true, false);
            Debug.Log("Golpe desde ADELANTE");
        }
            
        else if(forward < -0.5f)
        {
            _player.SetMovementCanceledOnCollision(false, true);
            Debug.Log("Golpe desde ATRÁS");
        }

        _timer += Time.fixedDeltaTime;
    }

    public void ArtificialCollisionExit(Collision other)
    {
        if(!TryResolveService()) return;

        if(!_player.MovementCanceledOnCollisionForward && !_player.MovementCanceledOnCollisionBack) return;

        _player.SetMovementCanceledOnCollision(false, false);
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
