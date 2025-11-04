using System;
using System.Collections.Generic;
using UnityEngine;

namespace Networking
{
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

            if (type.ToString().StartsWith("CTS"))
                Debug.LogWarning(
                    "Subscribed to packet type " + type + " in Client, but that should be a Client To Server packet.",
                    this);

            if (!_subscribers.ContainsKey(type) || _subscribers[type] == null)
                _subscribers[type] = new List<Action<BasePacket>>();
            _subscribers[type].Add(callback);
        }

        public void Unsubscribe<T>(Action<BasePacket> callback) where T : BasePacket
        {
            var type = typeof(T);
            if (_subscribers.ContainsKey(type))
                _subscribers[type].Remove(callback);
        }

        protected void HandlePacket(BasePacket packet)
        {
            Debug.Log($"[Client] Received packet from server: {packet.Type}");

            if (_subscribers.TryGetValue(packet.GetType(), out var callbacks))
                foreach (var callback in callbacks)
                    callback?.Invoke(packet);
        }
    }
}