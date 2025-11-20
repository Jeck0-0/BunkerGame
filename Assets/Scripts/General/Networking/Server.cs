using System;
using System.Collections.Generic;
using UnityEngine;

namespace Networking
{
    public abstract class Server : ScriptableVariable<Client>
    {
        protected Dictionary<Type, List<Action<uint, BasePacket>>> _subscribers = new();
        protected List<Action<uint, BasePacket>> _subscribedToAll = new();
        public event Action<uint> OnPlayerConnected;
        public event Action<uint> OnPlayerDisconnected;
        protected void InvokePlayerConnected(uint playerId) => OnPlayerConnected?.Invoke(playerId);
        protected void InvokePlayerDisconnected(uint playerId) => OnPlayerDisconnected?.Invoke(playerId);
        
        public abstract void Connect(int maxPlayers);
        public abstract void SendTo(uint user, BasePacket packet);
        public abstract void SendToAll(BasePacket packet);
        public abstract void SendToAllExcept(uint user, BasePacket packet);
        public abstract void Disconnect();
        public abstract void Update();

        public void Subscribe<T>(Action<uint, BasePacket> callback) where T : BasePacket
        {
            var type = typeof(T);

            if (type.ToString().StartsWith("STC"))
                Debug.LogWarning(
                    "Subscribed to packet type " + type + " in Server, but that should be a Server To Client packet.",
                    this);

            if (!_subscribers.ContainsKey(type) || _subscribers[type] == null)
                _subscribers[type] = new();
            _subscribers[type].Add(callback);
        }

        public void SubscribeToAll(Action<uint, BasePacket> callback)
        {
            _subscribedToAll.Add(callback);
        }

        public void Unsubscribe<T>(Action<uint, BasePacket> callback) where T : BasePacket
        {
            var type = typeof(T);
            if (_subscribers.ContainsKey(type))
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
}