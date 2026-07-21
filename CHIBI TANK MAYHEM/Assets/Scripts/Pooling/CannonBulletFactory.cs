using System.Collections.Generic;
using UnityEngine;

public class CannonBulletFactory : Factory<ShooteableObject, BulletType>
{
    public static CannonBulletFactory Instance { get; private set; }

    [SerializeField] private List<ShooteableObject> bulletPrefabs;
    [SerializeField] private int initialStockPerType = 15;

    private Dictionary<BulletType, ShooteableObject> _prefabsByName;
    private Dictionary<BulletType, ObjectPool<ShooteableObject>> _pools;

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        _prefabsByName = new();
        _pools = new();

        foreach(var prefab in bulletPrefabs)
            _prefabsByName[prefab.BulletType] = prefab;

        foreach(var type in _prefabsByName.Keys)
            GetOrCreatePool(type);
    }

    public override ShooteableObject Create(BulletType type, Vector3 position, Quaternion rotation)
    {
        var pool = GetOrCreatePool(type);
        var bullet = pool.Get();
        bullet.transform.SetPositionAndRotation(position, rotation);
        bullet.ResetState();
        return bullet;
    }

    public void Return(ShooteableObject bullet)
    {
        if(!_pools.TryGetValue(bullet.BulletType, out var pool))
        {
            GameManager.Instance.DestroyObject(bullet.gameObject);
            return;
        }

        pool.Return(bullet);
    }

    private ObjectPool<ShooteableObject> GetOrCreatePool(BulletType type)
    {
        if(_pools.TryGetValue(type, out var existingPool))
            return existingPool;

        if(!_prefabsByName.TryGetValue(type, out var prefab))
        {
            Debug.LogError($"CannonBulletFactory: no bullet prefab registered for '{type}'.");
            return null;
        }

        var pool = new ObjectPool<ShooteableObject>(
            () => Instantiate(prefab, transform),
            bullet => bullet.gameObject.SetActive(true),
            bullet => bullet.gameObject.SetActive(false),
            initialStockPerType);
            _pools[type] = pool;
        return pool;
    }
}
