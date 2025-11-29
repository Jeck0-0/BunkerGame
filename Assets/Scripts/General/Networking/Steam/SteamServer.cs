using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Steamworks;
using Steamworks.Data;
using UnityEngine;

namespace Networking
{
    public class SteamServer : GameServer, ISocketManager
    {
        protected static SteamServer SteamInstance => instance as SteamServer;
        public override int MaxPlayers { get; protected set; }
        
        public override int PlayerCount
        {
            get => SteamInstance != null ? SteamInstance._connections.Count : 0;
            protected set => Debug.LogWarning("Cannot change SteamServer's PlayerCount directly");
        }
        
        private SocketManager _socketManager;
        private Dictionary<uint, ServerConnectionInfo> _connections = new();

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

        
        public override void Create(int maxPlayers)
        {
            if (IsRunning)
            {
                Debug.LogWarning("Server already running!");
                return;
            }
            
            _socketManager = SteamNetworkingSockets.CreateRelaySocket(0, this);
            Debug.Log("[SERVER] Server started (relay mode)");
            
            this.MaxPlayers = maxPlayers;
            StartCoroutine(StartLobbyCoroutine());
            
            IsRunning = true;
            
            
            IEnumerator StartLobbyCoroutine()
            {
                var task = SteamLobby.Create(MaxPlayers);
            
                yield return new WaitUntil(() => task.IsCompleted);
                
                if (!task.Result)
                {
                    Debug.LogError("Failed to create lobby!");
                    Disconnect();
                }
                
                Debug.Log("[SERVER] Created Lobby");
                // Add myself
                _connections.Add(0, new ServerConnectionInfo(Steamworks.SteamClient.SteamId, new Connection()));
                _connectedIds.Add(0);
                InvokeOnPlayerConnected(0);
                
                SteamLobby.OpenInviteOverlay();
            }
        }


        public override void Disconnect()
        {
            if (!IsRunning) return;
            Debug.Log("[SERVER] Stopping server...");

            foreach (var connection in _connections.Values)
                connection.connection.Close();

            _connectedIds.Clear();
            _connections.Clear();
            _socketManager = null;
            IsRunning = false;
            SteamLobby.Leave();
            
            Debug.Log("[SERVER] Server stopped");
        }

        public override void SetOpen(bool open)
        {
            base.SetOpen(open);
            SteamLobby.CurrentLobby?.SetJoinable(open);
        }

        public override void Kick(uint playerId)
        {
            Debug.Log("[Server] Kicking player: " + playerId);
            if (_connections.ContainsKey(playerId))
            {
                _connections[playerId].connection.Close();
                _connections.Remove(playerId);
            }
            _connectedIds.Remove(playerId);
            InvokeOnPlayerDisconnected(playerId);
        }

        protected override void SendMessage(IEnumerable<uint> connectionId, BasePacket packet)
        {
            var data = GetData(packet);
            foreach (var id in connectionId)
            {
                if (id == 0)
                {
                    GameClient.instance.HandlePacket(packet);
                    continue;
                }
                
                _connections[id].connection.SendMessage(data, SendType.Reliable);
            }
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
                        _connectedIds.Remove(kvp.Key);
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

            if (!IsOpen)
            {
                Debug.LogWarning("[SERVER] Lobby closed! Rejecting connection.");
                connection.Close();
                return;
            }
            if (_connections.Count >= MaxPlayers)
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

            _connectedIds.Add(connectionId);
            _connections[connectionId] = new ServerConnectionInfo(steamId, connection);
            InvokeOnPlayerConnected(connectionId);

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
                _connectedIds.Remove(connectionId);
                InvokeOnPlayerDisconnected(connectionId);
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