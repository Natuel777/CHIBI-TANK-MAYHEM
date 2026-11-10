public sealed class GWorld
{
    private WorldStates _world;
    public WorldStates World => _world;

    public void Initialized()
    {
        _world = new WorldStates();
        ServiceLocator.Instance.Register<GWorld>(this);
    }
}