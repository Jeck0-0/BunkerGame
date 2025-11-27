using System;
using System.Collections.Generic;
using UnityEngine;

namespace Networking
{
    public abstract class GameClient : PersistentSingleton<GameClient>
    {
        public bool IsConnected => instance.isConnected;
        protected abstract bool isConnected { get; set; }

        /// <summary>
        /// This is called as soon as the client connects to the server.
        /// It's often best to subscribe to STC_JoinResponse instead of this.
        /// </summary>
        public static event Action OnConnect;
        protected void InvokeOnConnect() => OnConnect?.Invoke();
        
        public abstract void Connect(object args);
        public abstract void Disconnect();
        
        public static void Send(BasePacket packet) => instance?.SendLogic(packet);
        protected abstract void SendLogic(BasePacket packet);
        
        
#region Receiving Data

        protected static Dictionary<Type, List<Action<BasePacket>>> _subscribers = new();
        
        public static void Subscribe<T>(Action<BasePacket> callback) where T : BasePacket
        {
            var type = typeof(T);

            if (type.ToString().StartsWith("CTS"))
                Debug.LogWarning(
                    "Subscribed to packet type " + type + " in Client, but that should be a Client To Server packet.",
                    instance);

            if (!_subscribers.ContainsKey(type) || _subscribers[type] == null)
                _subscribers[type] = new List<Action<BasePacket>>();
            _subscribers[type].Add(callback);
        }

        public static void Unsubscribe<T>(Action<BasePacket> callback) where T : BasePacket
        {
            var type = typeof(T);
            if (_subscribers.ContainsKey(type))
                _subscribers[type].Remove(callback);
        }

        internal void HandlePacket(BasePacket packet)
        {
            Debug.Log($"[Client] Received packet from server: {packet.Type}");

            if (_subscribers.TryGetValue(packet.GetType(), out var callbacks))
                foreach (var callback in callbacks)
                    callback?.Invoke(packet);
        }

#endregion

    }
}