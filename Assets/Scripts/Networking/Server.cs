using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Server : ScriptableVariable<Client>
{
    protected Dictionary<Type, List<Action<uint, BasePacket>>> _subscribers = new();
    protected List<Action<uint, BasePacket>> _subscribedToAll = new();
    
    public abstract void Connect(int maxPlayers);
    public abstract void SendTo(uint user, BasePacket packet);
    public abstract void SendToAll(BasePacket packet);
    public abstract void SendToAllExcept(uint user, BasePacket packet);
    public abstract void Disconnect();
    public abstract void Update();

    public void Subscribe<T>(Action<uint, BasePacket> callback) where T : BasePacket
    {
        var type = typeof(T);
        if(!_subscribers.ContainsKey(type) || _subscribers[type] == null)
            _subscribers[type] = new ();
        _subscribers[type].Add(callback);
    }

    public void SubscribeToAll(Action<uint, BasePacket> callback)
    {
        _subscribedToAll.Add(callback);
    }

    public void Unsubscribe<T>(Action<uint, BasePacket> callback) where T : BasePacket
    {
        var type = typeof(T);
        if(_subscribers.ContainsKey(type))
            _subscribers[type].Remove(callback);
    }

    public void UnsubscribeFromAll(Action<uint, BasePacket> callback)
    {
        _subscribedToAll.Remove(callback);
    }
    
    protected void HandlePacket(uint connectionId, BasePacket packet)
    {
        Debug.Log($"[SERVER] Received packet from {connectionId}: {packet.Type}");
        
        if (_subscribers.TryGetValue(packet.GetType(), out var callbacks))
            foreach (var callback in callbacks)
                callback?.Invoke(connectionId, packet);
        
        foreach (var callback in _subscribedToAll)
            callback?.Invoke(connectionId, packet);
    }
}