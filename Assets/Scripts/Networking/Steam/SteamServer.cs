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
    [CreateAssetMenu(menuName = "ScriptableVariables/Networking/SteamServer", fileName = "SteamServer")]
    public class SteamServer : Server, ISocketManager
    {
        public bool IsRunning { get; private set; }
        public int PlayerCount => _clients.Count;

        private SocketManager _socketManager;
        private Dictionary<uint, Connection> _clients = new(); // Connection.Id -> Connection
        private Dictionary<uint, SteamId> _clientSteamIds = new(); // Connection.Id -> SteamId
        private int maxClients;

        public override void Connect(int maxPlayers)
        {
            StartServer();
        }

        public async Task StartServer(int maxPlayers = 4)
        {
            if (!SteamManager.TryInitialize())
            {
                Debug.LogError("Steam not initialized!");
                return;
            }

            if (IsRunning)
            {
                Debug.LogWarning("Server already running!");
                return;
            }

            _socketManager = SteamNetworkingSockets.CreateRelaySocket(0, this);

            IsRunning = true;
            Debug.Log("[SERVER] Server started (relay mode)");

            bool created = await SteamLobby.Create(maxPlayers);
            this.maxClients = maxPlayers;
            if (!created)
            {
                Debug.LogError("Failed to create lobby!");
                IsRunning = false;
                _socketManager.Close();
                _socketManager = null;
                return;
            }

            SteamMatchmaking.OnLobbyCreated += OnLobbyCreated;
            SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoined;
            SteamMatchmaking.OnLobbyMemberLeave += OnLobbyMemberLeft;
        }

        public override void Disconnect()
        {
            if (!IsRunning)
                return;

            foreach (var connection in _clients.Values)
                connection.Close();

            _clients.Clear();
            _clientSteamIds.Clear();
            _socketManager = null;
            IsRunning = false;
            SteamLobby.Leave();

            SteamMatchmaking.OnLobbyCreated += OnLobbyCreated;
            SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoined;
            SteamMatchmaking.OnLobbyMemberLeave += OnLobbyMemberLeft;

            Debug.Log("[SERVER] Server stopped");
        }

        public override void Update()
        {
            _socketManager?.Receive();
        }

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
            foreach (var kvp in _clientSteamIds)
            {
                if (kvp.Value == friend.Id)
                {
                    if (_clients.TryGetValue(kvp.Key, out var connection))
                    {
                        connection.Close();
                        _clients.Remove(kvp.Key);
                        _clientSteamIds.Remove(kvp.Key);
                    }

                    break;
                }
            }
        }



        public void OnConnecting(Connection connection, ConnectionInfo info)
        {
            Debug.Log($"[SERVER] Client connecting: {info.Identity.SteamId}");

            if (_clients.Count >= maxClients)
            {
                Debug.LogWarning("[SERVER] Server full! Rejecting connection.");
                connection.Close();
                return;
            }

            // Check if they're in the lobby
            if (SteamLobby.IsInLobby)
            {
                bool inLobby = false;
                foreach (var member in SteamLobby.CurrentLobby!.Value.Members)
                {
                    if (member.Id == info.Identity.SteamId)
                    {
                        inLobby = true;
                        break;
                    }
                }

                if (!inLobby)
                {
                    Debug.LogWarning($"[SERVER] {info.Identity.SteamId} not in lobby! Rejecting.");
                    connection.Close();
                    return;
                }
            }
        }

        public void OnConnected(Connection connection, ConnectionInfo info)
        {
            uint connectionId = connection.Id;
            SteamId steamId = info.Identity.SteamId;

            _clients[connectionId] = connection;
            _clientSteamIds[connectionId] = steamId;

            Debug.Log($"[SERVER] Client connected: {steamId} (Connection ID: {connectionId})");
            Debug.Log($"[SERVER] Total clients: {_clients.Count}");
        }

        public void OnDisconnected(Connection connection, ConnectionInfo info)
        {
            uint connectionId = connection.Id;

            if (_clientSteamIds.TryGetValue(connectionId, out var steamId))
            {
                Debug.Log($"[SERVER] Client disconnected: {steamId} (Reason: {info.EndReason})");
                _clientSteamIds.Remove(connectionId);
            }

            _clients.Remove(connectionId);
            Debug.Log($"[SERVER] Total clients: {_clients.Count}");
        }

        public void OnMessage(Connection connection, NetIdentity identity, IntPtr data, int size, long messageNum,
            long recvTime,
            int channel)
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



        public override void SendTo(uint connectionId, BasePacket packet)
        {

            if (!_clients.TryGetValue(connectionId, out var connection))
            {
                Debug.LogWarning($"Connection {connectionId} not found!");
                return;
            }

            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);
            packet.Serialize(bw);

            byte[] data = ms.ToArray();
            connection.SendMessage(data, SendType.Reliable);
        }

        public override void SendToAll(BasePacket packet)
        {
            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);
            packet.Serialize(bw);

            byte[] data = ms.ToArray();

            foreach (var connection in _clients.Values)
            {
                connection.SendMessage(data, SendType.Reliable);
            }
        }



        public override void SendToAllExcept(uint excludeConnectionId, BasePacket packet)
        {
            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);
            packet.Serialize(bw);

            byte[] data = ms.ToArray();

            foreach (var kvp in _clients)
            {
                if (kvp.Key != excludeConnectionId)
                {
                    kvp.Value.SendMessage(data, SendType.Reliable);
                }
            }
        }
    }
}