using System;
using System.IO;
using System.Runtime.InteropServices;
using Steamworks;
using Steamworks.Data;
using UnityEngine;

namespace Networking
{
    public class SteamClient : GameClient, IConnectionManager
    {
        protected static SteamClient SteamInstance => instance as SteamClient;
        protected override bool isConnected => (SteamInstance?.Connection != null && SteamInstance?.Connection.Value.Id != 0) || SteamLobby.IsOwner;
        
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

        public void Update()
        {
            _connectionManager?.Receive();
        }

        public void Join(SteamId lobbyId)
        {
            SteamMatchmaking.JoinLobbyAsync(lobbyId);
        }
        
        public override void Disconnect()
        {
            if (IsConnected)
            {
                Connection?.Close();
                _connectionManager = null;
            }

            SteamLobby.Leave();
        }



#region Lobby
        private void OnLobbyCreated(Result result, Lobby lobby)
        {
            Debug.Log($"[Client] Connecting to local host");
            if (result == Result.OK)
                InvokeOnConnect();
        }
        private void OnJoinedLobby(Lobby lobby)
        {
            if (SteamLobby.IsOwner) return;
            
            Debug.Log($"[Client] Joined lobby, connecting to host: {lobby.Owner.Id}");
            
            if (IsConnected)
            {
                Debug.LogWarning("[Client] Nevermind, already connected!");
                return;
            }

            _connectionManager = SteamNetworkingSockets.ConnectRelay(lobby.Owner.Id, 0, this);
        }
#endregion
        
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
            InvokeOnConnect();
        }

        public void OnDisconnected(ConnectionInfo info)
        {
            Debug.Log($"[CLIENT] Disconnected from server: {info.EndReason}");
        }
#endregion

#region Sending Data
        protected override void SendLogic(BasePacket packet)
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
                GameServer.instance.HandlePacket(0, packet);
                return;
            }
                    
            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);
            packet.Serialize(bw);

            byte[] data = ms.ToArray();
            SteamInstance?.Connection?.SendMessage(data, SendType.Reliable);
        }
#endregion


    }
}