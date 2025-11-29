using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Networking
{
    public abstract class GameServer : PersistentSingleton<GameServer>
    {
        public bool IsRunning { get; protected set; }
        public abstract int PlayerCount { get; protected set; }
        public abstract int MaxPlayers { get; protected set; }
        public static bool IsOpen { get; private set; } = true;
        
        public static event Action<uint> OnPlayerConnected;
        public static event Action<uint> OnPlayerDisconnected;
        public static event Action OnServerStarted;

        protected void InvokeOnPlayerConnected(uint connectionId) => OnPlayerConnected?.Invoke(connectionId);
        protected void InvokeOnPlayerDisconnected(uint connectionId) => OnPlayerDisconnected?.Invoke(connectionId);
        protected void InvokeOnServerStarted() => OnServerStarted?.Invoke();

        public virtual void SetOpen(bool open) => IsOpen = open;

        public abstract void Create(int maxPlayers);
        
        public abstract void Disconnect();
        
        public abstract void Kick(uint playerId);
        

#region Sending Data

        protected List<uint> _connectedIds = new();
        
        public static void SendTo(uint connectionId, BasePacket packet)
        {
            if (!HasInstance || !instance._connectedIds.Contains(connectionId))
            {
                Debug.LogWarning($"Connection {connectionId} not found!");
                return;
            }
            instance.SendMessage(new[] { connectionId }, packet);
        }

        public static void SendToAll(BasePacket packet)
        {
            instance?.SendMessage(instance._connectedIds, packet);
        }
        
        public static void SendToAllExcept(uint excludeConnectionId, BasePacket packet)
        {
            instance?.SendMessage(instance._connectedIds
                .Where(x => x != excludeConnectionId),
                packet);
        }


        protected byte[] GetData(BasePacket packet)
        {
            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);
            packet.Serialize(bw);

            return ms.ToArray();
        }

        protected abstract void SendMessage(IEnumerable<uint> connectionId, BasePacket packet);
#endregion

#region Receiving Data

        protected static Dictionary<Type, List<Action<uint, BasePacket>>> _subscribers = new();
        protected static List<Action<uint, BasePacket>> _subscribedToAll = new();

        public static void Subscribe<T>(Action<uint, BasePacket> callback) where T : BasePacket
        {
            var type = typeof(T);

            if (type.ToString().StartsWith("STC"))
                Debug.LogWarning("Subscribed to packet type " + type + " in Server, but that should be a Server To Client packet.", instance);

            if (!_subscribers.ContainsKey(type) || _subscribers[type] == null)
                _subscribers[type] = new();
            _subscribers[type].Add(callback);
        }

        public static void SubscribeToAll(Action<uint, BasePacket> callback)
        {
            _subscribedToAll.Add(callback);
        }

        public static void Unsubscribe<T>(Action<uint, BasePacket> callback) where T : BasePacket
        {
            var type = typeof(T);
            if (_subscribers.ContainsKey(type))
                _subscribers[type].Remove(callback);
        }

        public static void UnsubscribeFromAll(Action<uint, BasePacket> callback)
        {
            _subscribedToAll.Remove(callback);
        }

        internal void HandlePacket(uint connectionId, BasePacket packet)
        {
            Debug.Log($"[SERVER] Received packet from {connectionId}: {packet.Type}");

            if (_subscribers.TryGetValue(packet.GetType(), out var callbacks))
                foreach (var callback in callbacks)
                    callback?.Invoke(connectionId, packet);

            foreach (var callback in _subscribedToAll)
                callback?.Invoke(connectionId, packet);
        }
#endregion


    }
}