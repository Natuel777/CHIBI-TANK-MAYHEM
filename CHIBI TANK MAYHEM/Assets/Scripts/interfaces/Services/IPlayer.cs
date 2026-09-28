using UnityEngine;

public interface IPlayer
{
    Transform transform { get; }
    bool MovementCanceledOnCollisionForward { get; }
    bool MovementCanceledOnCollisionBack { get; }
    void SetMovementCanceledOnCollision(bool forward, bool back);
}

public interface IServiceConsumer
{
    bool TryResolveService();
}
