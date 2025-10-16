using System;
using System.Collections.Generic;

public abstract class Client : ScriptableVariable<Client>
{
    protected Dictionary<Type, List<Action<BasePacket>>> _subscribers = new();
    
    public abstract void Connect();
    public abstract void Send(BasePacket packet);
    public abstract void Disconnect();
    public abstract void Update();
    
    public void Subscribe<T>(Action<BasePacket> callback) where T : BasePacket
    {
        var type = typeof(T);
        if(!_subscribers.ContainsKey(type) || _subscribers[type] == null)
            _subscribers[type] = new List<Action<BasePacket>>();
        _subscribers[type].Add(callback);
    }

    public void Unsubscribe<T>(Action<BasePacket> callback) where T : BasePacket
    {
        var type = typeof(T);
        if(_subscribers.ContainsKey(type))
            _subscribers[type].Remove(callback);
    }
}