using UnityEngine;

public interface IGameManager
{
    void DestroyObject(GameObject obj);
    LevelManager LevelManager { get; }
}
