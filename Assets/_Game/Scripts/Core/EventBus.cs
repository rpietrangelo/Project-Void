using System;
using System.Collections.Generic;
using UnityEngine;

public static class EventBus
{
    private static readonly Dictionary<Type, Delegate> _handlers = new();

    public static void Subscribe<T>(Action<T> handler)
    {
        if (_handlers.TryGetValue(typeof(T), out var existing))
            _handlers[typeof(T)] = Delegate.Combine(existing, handler);
        else
            _handlers[typeof(T)] = handler;
    }

    public static void Unsubscribe<T>(Action<T> handler)
    {
        if (_handlers.TryGetValue(typeof(T), out var existing))
        {
            var updated = Delegate.Remove(existing, handler);
            if (updated == null)
                _handlers.Remove(typeof(T));
            else
                _handlers[typeof(T)] = updated;
        }
    }

    public static void Publish<T>(T evt)
    {
        if (_handlers.TryGetValue(typeof(T), out var handler))
            ((Action<T>)handler)?.Invoke(evt);
    }

    public static void Clear() => _handlers.Clear();
}
