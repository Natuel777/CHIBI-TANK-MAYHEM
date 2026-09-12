public interface IPlayer
{
    bool MovementCanceledOnCollision { get; }
    void SetMovementCanceledOnCollision(bool value);
}

public interface IServiceConsumer
{
    bool TryResolveService();
}
