using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Sirenix.OdinInspector;
using Unity.VisualScripting;

namespace Networking
{
    public class LocalServer : GameServer
    {
        public override int PlayerCount { get; protected set; }
        public override int MaxPlayers { get; protected set; }

        [ShowInInspector, ReadOnly, DoNotSerialize] protected Dictionary<uint, Socket> connectionInfos = new();
        protected static uint idCounter;

        Socket serverSocket;
        int port = 6969;
        
        public void Update()
        {
            if (!IsRunning) return;
            
            if(IsOpen)
                AcceptNewConnection();
            ReceivePackets();
        }

        protected void AcceptNewConnection()
        {
            try
            {
                var newSocket = serverSocket.Accept();
                var newId = idCounter++;
                connectionInfos.Add(newId, newSocket);
                _connectedIds.Add(newId);
                InvokeOnPlayerConnected(newId);
                Debug.Log("[Server] Local client connected");
            }
            catch (Exception e)
            {
                if (e is not SocketException)
                    Debug.LogError(e);
            }
        }

        protected void ReceivePackets()
        {
            try
            {
                foreach (var kvp in connectionInfos)
                {
                    while (kvp.Value.Available > 0)
                    {
                        byte[] buffer = new byte[kvp.Value.Available];
                        kvp.Value.Receive(buffer);

                        var rms = new MemoryStream(buffer);
                        var br = new BinaryReader(rms);

                        while (rms.Position < rms.Length)
                        {
                            var packet = BasePacket.DeserializePacket(br);
                            HandlePacket(kvp.Key, packet);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error receiving the packet: " + e);
            }
        }
        
        public override void Create(int maxPlayers)
        {
            this.MaxPlayers = maxPlayers;
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Blocking = false;
            serverSocket.Bind(new IPEndPoint(IPAddress.Any, port));
            serverSocket.Listen(5);
            IsRunning = true;
        }

        public override void Disconnect()
        {
            foreach (var connection in connectionInfos.Values)
                connection.Close();
            serverSocket?.Close();
            connectionInfos.Clear();
            _connectedIds.Clear();
            IsRunning = false;
        }

        protected override void SendMessage(IEnumerable<uint> connectionId, BasePacket packet)
        {
            foreach (uint connection in connectionId)
                connectionInfos[connection].Send(GetData(packet));
        }
    }
}
