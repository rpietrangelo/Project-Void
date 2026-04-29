using System;
using System.Collections.Generic;
using UnityEngine;

public class ServiceLocator : MonoBehaviour
{
    private static ServiceLocator _instance;
    private readonly Dictionary<Type, object> _services = new();

    public static ServiceLocator Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("[ServiceLocator]");
                _instance = go.AddComponent<ServiceLocator>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Register<T>(T service) where T : class
    {
        _services[typeof(T)] = service;
        Debug.Log($"[ServiceLocator] Registered: {typeof(T).Name}");
    }

    public T Get<T>() where T : class
    {
        if (_services.TryGetValue(typeof(T), out var service))
            return (T)service;
        Debug.LogError($"[ServiceLocator] Service not found: {typeof(T).Name}");
        return null;
    }

    public bool IsRegistered<T>() where T : class => _services.ContainsKey(typeof(T));

    public void Unregister<T>() where T : class => _services.Remove(typeof(T));
}
