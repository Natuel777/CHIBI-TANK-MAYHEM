using UnityEngine;
using System.Collections.Generic;
using System;

public sealed class ServiceLocator
{
    private static ServiceLocator _instance;
    public static ServiceLocator Instance => _instance ??= new ServiceLocator();
    private readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

    public bool Register<T>(T service)
    {
        var type = typeof(T);

        if(_services.TryAdd(type, service))
            return true;

        Debug.LogWarning($"[SERVICE LOCATOR] Service of type {type} is already registered.");
        return false;
    }

    public bool Unregister<T>(T service)
    {
        if(!_services.TryGetValue(typeof(T), out var existingService) || !ReferenceEquals(existingService, service))
        {
            Debug.LogWarning($"[SERVICE LOCATOR] Service of type {typeof(T)} is not registered or does not match the provided instance.");
            return false;
        }

        _services.Remove(typeof(T));
        return true;
    }

    public bool TryGet<T>(out T service)
    {
        if(_services.TryGetValue(typeof(T), out var existingService))
        {
            service = (T)existingService;
            return true;
        }

        Debug.LogWarning($"[SERVICE LOCATOR] Service of type {typeof(T)} is not registered.");
        service = default;
        return false;
    }
}
