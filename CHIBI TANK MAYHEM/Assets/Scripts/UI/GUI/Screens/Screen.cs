using UnityEngine;

public class Screen : MonoBehaviour, IScreen
{
    [SerializeField] protected Screen nextScreen, backScreen;

    public virtual void Activate() 
    {
        if(!gameObject.activeSelf)
            gameObject.SetActive(true);
    }
    
    public virtual void Deactivate() {gameObject.SetActive(false);}
}
