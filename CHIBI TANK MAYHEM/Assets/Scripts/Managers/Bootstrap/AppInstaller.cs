using UnityEngine;
using System.Threading.Tasks;

public class AppInstaller : MonoBehaviour, IServiceConsumer
{
    private bool _installed = false;

    public Task Install()
    {
        if(_installed) return Task.CompletedTask;

        if(TryResolveService())
        {
           return Task.CompletedTask;
        } 

        ServiceLocator.Instance.Register(gameObject.AddComponent<SceneLoader>());
        _installed = true;
        DontDestroyOnLoad(gameObject);
        Debug.Log("AppInstaller: servicios registrados.");
        return Task.CompletedTask;
    }

    public bool TryResolveService()
    {
        if(ServiceLocator.Instance.TryGet<SceneLoader>(out _))
            return true;

        return false;
    }

    private void OnDestroy()
    {
        if(_installed)
            ServiceLocator.Instance.Clear();
    }
}
