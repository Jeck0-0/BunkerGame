using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Steamworks;
using Steamworks.Data;
using UnityEngine;

namespace Networking
{
    public class SteamServer : PersistentSingleton<SteamServer>, ISocketManager
    {
        public static bool IsRunning { get; private set; }
        public static int PlayerCount => instance != null ? instance._connections.Count : 0;

        public static event Action<uint> OnPlayerConnected;
        public static event Action<uint> OnPlayerDisconnected;
        public static event Action OnServerStarted;
        
        private SocketManager _socketManager;
        private Dictionary<uint, ServerConnectionInfo> _connections = new(); // Connection.Id -> Connection
        private int maxClients;

        protected override void Awake()
        {
            base.Awake();
            SteamManager.TryInitialize();
            SteamMatchmaking.OnLobbyCreated += OnLobbyCreated;
            SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoined;
            SteamMatchmaking.OnLobbyMemberLeave += OnLobbyMemberLeft;
        }

        private void OnApplicationQuit()
        {
            Disconnect();
            SteamMatchmaking.OnLobbyCreated -= OnLobbyCreated;
            SteamMatchmaking.OnLobbyMemberJoined -= OnLobbyMemberJoined;
            SteamMatchmaking.OnLobbyMemberLeave -= OnLobbyMemberLeft;
        }
        
        protected void Update()
        {
            _socketManager?.Receive();
        }

        
        public void Create(int maxPlayers)
        {
            if (IsRunning)
            {
                Debug.LogWarning("Server already running!");
                return;
            }
            
            _socketManager = SteamNetworkingSockets.CreateRelaySocket(0, this);
            Debug.Log("[SERVER] Server started (relay mode)");
            
            this.maxClients = maxPlayers;
            StartCoroutine(StartLobbyCoroutine());
            
            IsRunning = true;
            
            
            IEnumerator StartLobbyCoroutine()
            {
                var task = SteamLobby.Create(maxClients);
            
                yield return new WaitUntil(() => task.IsCompleted);
                
                if (!task.Result)
                {
                    Debug.LogError("Failed to create lobby!");
                    Disconnect();
                }
                
                Debug.Log("[SERVER] Created Lobby");
                // Add myself
                _connections.Add(0, new ServerConnectionInfo(Steamworks.SteamClient.SteamId, new Connection()));
                OnPlayerConnected?.Invoke(0);
            }
        }


        public void Disconnect()
        {
            if (!IsRunning) return;
            Debug.Log("[SERVER] Stopping server...");

            foreach (var connection in _connections.Values)
                connection.connection.Close();

            _connections.Clear();
            _socketManager = null;
            IsRunning = false;
            SteamLobby.Leave();
            
            Debug.Log("[SERVER] Server stopped");
        }


#region Lobby
        private void OnLobbyCreated(Result result, Lobby lobby)
        {
            Debug.Log($"[SERVER] Lobby created! ID: {lobby.Id}");
            Debug.Log($"[SERVER] Players will connect via SteamId: {Steamworks.SteamClient.SteamId}");
        }

        private void OnLobbyMemberJoined(Lobby lobby, Friend friend)
        {
            // Don't log ourselves
            if (friend.Id == Steamworks.SteamClient.SteamId)
                return;

            Debug.Log($"[SERVER] {friend.Name} joined lobby (waiting for socket connection...)");
            // They will connect via socket automatically when they join the lobby
        }

        private void OnLobbyMemberLeft(Lobby lobby, Friend friend)
        {
            Debug.Log($"[SERVER] {friend.Name} left lobby");

            // Find and disconnect their socket connection
            foreach (var kvp in _connections)
            {
                if (kvp.Value.SteamId == friend.Id)
                {
                    if (_connections.TryGetValue(kvp.Key, out var connection))
                    {
                        connection.connection.Close();
                        _connections.Remove(kvp.Key);
                    }
                    break;
                }
            }
        }
#endregion

#region ISocketManager

        public void OnConnecting(Connection connection, ConnectionInfo info)
        {
            Debug.Log($"[SERVER] Client connecting: {info.Identity.SteamId}");

            if (_connections.Count >= maxClients)
            {
                Debug.LogWarning("[SERVER] Server full! Rejecting connection.");
                connection.Close();
                return;
            }

            // Check if they're in the lobby
            if (SteamLobby.IsInLobby)
            {
                bool inLobby = SteamLobby.CurrentLobby!.Value.Members.Any(x=>x.Id == info.Identity.SteamId);
                if (!inLobby)
                {
                    Debug.LogWarning($"[SERVER] {info.Identity.SteamId} not in lobby! Rejecting.");
                    connection.Close();
                    return;
                }
            }

            connection.Accept();
        }

        public void OnConnected(Connection connection, ConnectionInfo info)
        {
            uint connectionId = connection.Id;
            SteamId steamId = info.Identity.SteamId;

            _connections[connectionId] = new ServerConnectionInfo(steamId, connection);
            OnPlayerConnected?.Invoke(connectionId);

            Debug.Log($"[SERVER] Client connected: {steamId} (Connection ID: {connectionId})");
            Debug.Log($"[SERVER] Total clients: {_connections.Count}");
        }

        public void OnDisconnected(Connection connection, ConnectionInfo info)
        {
            uint connectionId = connection.Id;

            if (_connections.TryGetValue(connectionId, out var connectionInfo))
            {
                Debug.Log($"[SERVER] Client disconnected: {connectionInfo} (Reason: {info.EndReason})");
                _connections.Remove(connectionId);
                OnPlayerDisconnected?.Invoke(connectionId);
            }

            Debug.Log($"[SERVER] Total clients: {_connections.Count}");
        }

        public void OnMessage(Connection connection, NetIdentity identity, IntPtr data, int size, long messageNum,
            long recvTime, int channel)
        {
            byte[] buffer = new byte[size];
            Marshal.Copy(data, buffer, 0, size);

            using var ms = new MemoryStream(buffer);
            using var br = new BinaryReader(ms);

            while (ms.Position < ms.Length)
            {
                var packet = BasePacket.DeserializePacket(br);
                HandlePacket(connection.Id, packet);
            }
        }
#endregion

#region Sending Data
        public static void SendTo(uint connectionId, BasePacket packet)
        {
            if (!HasInstance || !instance._connections.ContainsKey(connectionId))
            {
                Debug.LogWarning($"Connection {connectionId} not found!");
                return;
            }
            instance.SendMessage(new[] { connectionId }, packet);
        }

        public static void SendToAll(BasePacket packet)
        {
            instance?.SendMessage(instance._connections.Keys, packet);
        }
        
        public static void SendToAllExcept(uint excludeConnectionId, BasePacket packet)
        {
            instance?.SendMessage(instance._connections
                .Where(x => x.Key != excludeConnectionId)
                .Select(x => x.Key), 
                packet);
        }


        protected byte[] GetData(BasePacket packet)
        {
            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);
            packet.Serialize(bw);

            return ms.ToArray();
        }

        protected void SendMessage(IEnumerable<uint> connectionId, BasePacket packet)
        {
            var data = GetData(packet);
            foreach (var id in connectionId)
            {
                if (id == 0)
                {
                    SteamClient.instance.HandlePacket(packet);
                    continue;
                }
                
                _connections[id].connection.SendMessage(data, SendType.Reliable);
            }
        }
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

        protected class ServerConnectionInfo
        {
            public SteamId SteamId;
            public Connection connection;
            public ServerConnectionInfo() {}
            public ServerConnectionInfo(SteamId steamId, Connection connection)
                { SteamId = steamId; this.connection = connection; }
        }

    }
}