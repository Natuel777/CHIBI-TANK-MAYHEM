public sealed class GWorld
{
    private WorldStates _world;
    private bool _initialized = false;
    public WorldStates World => _world;
    public bool Initialized => _initialized;

    public void Initialize()
    {
        _world = new WorldStates();
        ServiceLocator.Instance.Register<GWorld>(this);
        _initialized = true;
    }
}