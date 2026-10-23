using UnityEngine;

public class FlatTankMeshSetter : MonoBehaviour, IMesh
{
    public void Initialize()
    {
        if(TryGetComponent(out Rigidbody rigidbody))
        {
            rigidbody.centerOfMass = new Vector3(0f, -0.5f, 2f);
        }
    }
}
