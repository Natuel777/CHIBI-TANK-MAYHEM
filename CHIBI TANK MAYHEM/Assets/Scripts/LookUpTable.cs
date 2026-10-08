using System.Collections.Generic;

public class LookupTable<TKey, TValue>
{
    private readonly Dictionary<TKey, TValue> table = new();

    public void Add(TKey key, TValue value) => table[key] = value;

    public bool TryGet(TKey key, out TValue value) => table.TryGetValue(key, out value);

    public TValue Get(TKey key) => table[key];

    public bool Contains(TKey key) => table.ContainsKey(key);

    public void Remove(TKey key) => table.Remove(key);

    public void Clear() => table.Clear();
}