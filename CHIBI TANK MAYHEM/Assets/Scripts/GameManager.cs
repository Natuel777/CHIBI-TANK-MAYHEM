using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if(Instance != null && Instance != this)
            Destroy(this.gameObject);
        
        else Instance = this;
    }

    public void DestroyObject(GameObject obj)
    {
        Destroy(obj);
        #if UNITY_EDITOR
        Debug.Log("Destroyed object: " + obj.name);
        #endif
    }
}
