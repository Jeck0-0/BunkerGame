using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Steamworks;
using Steamworks.Data;
using UnityEngine;

namespace Networking
{
    public class SteamClient : PersistentSingleton<SteamClient>, IConnectionManager
    {
        public static bool IsConnected => (instance?.Connection != null && instance?.Connection.Value.Id != 0) || SteamLobby.IsOwner;

        public static event Action OnConnect;
        
        private ConnectionManager _connectionManager;
        private Connection? Connection => _connectionManager?.Connection;

        protected override void Awake()
        {
            base.Awake();
            
            if (!SteamManager.TryInitialize()) return;
            
            SteamMatchmaking.OnLobbyEntered += OnJoinedLobby;
            SteamMatchmaking.OnLobbyCreated += OnLobbyCreated;
            SteamLobby.Initialize();
        }

        private void OnApplicationQuit()
        {
            SteamMatchmaking.OnLobbyEntered -= OnJoinedLobby;
            SteamMatchmaking.OnLobbyCreated -= OnLobbyCreated;
            Disconnect();
        }

        public static void Send(BasePacket packet)
        {
            if (!HasInstance)
            {
                Debug.LogWarning("Steam Client is not initialized!");
                return;
            }
            if (!IsConnected)
            {
                Debug.LogWarning("Not connected to host!");
                return;
            }

            //If I'm hosting, don't send through network
            if (SteamLobby.IsOwner)
            {
                SteamServer.instance.HandlePacket(0, packet);
                return;
            }
            
            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);
            packet.Serialize(bw);

            byte[] data = ms.ToArray();
            instance?.Connection?.SendMessage(data, SendType.Reliable);
        }


        public void Disconnect()
        {
            if (IsConnected)
            {
                Connection?.Close();
                _connectionManager = null;
            }

            SteamLobby.Leave();
        }

        public void Update()
        {
            _connectionManager?.Receive();
        }



        private void OnLobbyCreated(Result result, Lobby lobby)
        {
            Debug.Log($"[Client] Connecting to local host");
            if (result == Result.OK)
                OnConnect?.Invoke();
        }
        private void OnJoinedLobby(Lobby lobby)
        {
            Debug.Log($"[Client] Joined lobby, connecting to host...");
            if (IsConnected)
            {
                Debug.LogWarning("Already connected!");
                return;
            }

            try
            {
                _connectionManager = SteamNetworkingSockets.ConnectRelay(lobby.Owner.Id, 0, this);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            OnConnect?.Invoke();
            Debug.Log($"[Client] Connecting to host: {lobby.Owner.Id}");
        }
        
        
#region IConnectionManager
        public void OnMessage(IntPtr data, int size, long messageNum, long recvTime, int channel)
        {
            byte[] buffer = new byte[size];
            Marshal.Copy(data, buffer, 0, size);

            using var ms = new MemoryStream(buffer);
            using var br = new BinaryReader(ms);

            while (ms.Position < ms.Length)
            {
                var packet = BasePacket.DeserializePacket(br);
                HandlePacket(packet);
            }
        }
        
        public void OnConnecting(ConnectionInfo info)
        {
            Debug.Log($"[CLIENT] Connecting to server...");
        }

        public void OnConnected(ConnectionInfo info)
        {
            Debug.Log($"[CLIENT] Connected to server! Connection ID: {Connection?.Id}");
        }

        public void OnDisconnected(ConnectionInfo info)
        {
            Debug.Log($"[CLIENT] Disconnected from server: {info.EndReason}");
        }
#endregion

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