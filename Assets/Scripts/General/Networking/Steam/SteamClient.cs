using System;
using System.IO;
using System.Runtime.InteropServices;
using Steamworks;
using Steamworks.Data;
using UnityEngine;

namespace Networking
{
    [CreateAssetMenu(menuName = "ScriptableVariables/Networking/SteamClient", fileName = "SteamClient")]
    public class SteamClient : Client, IConnectionManager
    {

        private ConnectionManager _connectionManager;
        private Connection Connection => _connectionManager.Connection;

        public bool IsConnected => Connection.Id != 0;

        public override void Connect()
        {
            if (!SteamManager.TryInitialize())
            {
                var go = new GameObject("SteamManager");
                go.AddComponent<SteamManager>();
                Debug.LogWarning("Steamworks not initialized");

                if (!Steamworks.SteamClient.IsValid)
                {
                    Debug.LogError("Steamworks could not be initialized");
                    return;
                }
            }


            SteamLobby.Initialize();
            SteamMatchmaking.OnLobbyEntered += OnJoinedLobby;
        }

        public override void Send(BasePacket packet)
        {
            if (!IsConnected)
            {
                Debug.LogWarning("Not connected to host!");
                return;
            }

            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);
            packet.Serialize(bw);

            byte[] data = ms.ToArray();
            Connection.SendMessage(data, SendType.Reliable);
        }

        public override void Disconnect()
        {
            if (IsConnected)
            {
                Connection.Close();
                _connectionManager = null;
            }

            SteamMatchmaking.OnLobbyEntered -= OnJoinedLobby;
            SteamLobby.Leave();
        }

        public override void Update()
        {
        }



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



        private void OnJoinedLobby(Lobby lobby)
        {
            Debug.Log($"[CLIENT] Joined lobby, connecting to host...");
            if (IsConnected)
            {
                Debug.LogWarning("Already connected!");
                return;
            }

            var hostId = lobby.Owner.Id;

            _connectionManager = SteamNetworkingSockets.ConnectRelay(hostId, 3000, this);

            Debug.Log($"Connecting to host: {hostId}");
        }

        public void OnConnecting(ConnectionInfo info)
        {
            Debug.Log($"[CLIENT] Connecting to server...");
        }

        public void OnConnected(ConnectionInfo info)
        {
            Debug.Log($"[CLIENT] Connected to server! Connection ID: {Connection.Id}");
        }

        public void OnDisconnected(ConnectionInfo info)
        {
            Debug.Log($"[CLIENT] Disconnected from server: {info.EndReason}");
        }

    }
}