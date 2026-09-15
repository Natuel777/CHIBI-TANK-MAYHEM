using UnityEngine;
using System.Collections.Generic;

[DefaultExecutionOrder(-400)]
public class ChibiSoldierFactory : Factory<ChibiSoldier, ChibiSoldierType>
{
    [SerializeField] private List<ChibiSoldier> _chibiSoldiersPrefabs;
    [SerializeField] private int _initialStockPerType = 15;
    
    private Dictionary<ChibiSoldierType, ChibiSoldier> _prefabsByType;
    private Dictionary<ChibiSoldierType, ObjectPool<ChibiSoldier>> _pools;
    private IGameManager _gameManager;

    private void Awake()
    {
        ServiceLocator.Instance.Register<ChibiSoldierFactory>(this);

        if(ServiceLocator.Instance.TryGet(out IGameManager gmInterface))
        {
            if(gmInterface is GameManager gameManager)
                _gameManager = gameManager;
        }

        _prefabsByType = new();
        _pools = new();

        foreach(var prefab in _chibiSoldiersPrefabs)
            _prefabsByType[prefab.ChibiSoldierType] = prefab;

        foreach(var type in _prefabsByType.Keys)
            GetOrCreatePool(type);
    }

    public override ChibiSoldier Create(ChibiSoldierType type, Vector3 position, Quaternion rotation)
    {
        var pool = GetOrCreatePool(type);
        var soldier = pool.Get();
        soldier.transform.SetPositionAndRotation(position, rotation);
        return soldier;
    }

    public void Return(ChibiSoldier soldier)
    {
        if(!_pools.TryGetValue(soldier.ChibiSoldierType, out var pool))
        {
            _gameManager?.DestroyObject(soldier.gameObject);
            return;
        }

        pool.Return(soldier);
    }

    private ObjectPool<ChibiSoldier> GetOrCreatePool(ChibiSoldierType type)
    {
        if(_pools.TryGetValue(type, out var existingPool))
            return existingPool;

        if(!_prefabsByType.TryGetValue(type, out var prefab))
        {
            Debug.LogError($"CannonBulletFactory: no bullet prefab registered for '{type}'.");
            return null;
        }

        var pool = new ObjectPool<ChibiSoldier>(
            () => Instantiate(prefab, transform),
            soldier => soldier.gameObject.SetActive(true),
            soldier => soldier.gameObject.SetActive(false),
            _initialStockPerType);
            _pools[type] = pool;
        return pool;
    }
}
