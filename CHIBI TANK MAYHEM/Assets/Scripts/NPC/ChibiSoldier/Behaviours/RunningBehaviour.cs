using UnityEngine;

public class RunningBehaviour : IBehaviours
{
    private bool _active;
    public RunningBehaviour() {}

    public void Active(bool value) {_active = value;}

    public void ArtificialUpdate()
    {
        if(!_active) return;
    }
}
