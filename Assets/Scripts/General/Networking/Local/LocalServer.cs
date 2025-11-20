using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;

namespace Networking
{
    [CreateAssetMenu(menuName = "ScriptableVariables/Networking/LocalServer", fileName = "LocalServer")]
    public class LocalServer : Server
    {
        Socket serverSocket;
        int port = 6969;

        [ShowInInspector, ReadOnly, DoNotSerialize] private List<ConnectionInfo> connectionInfos = new();

        private string currentLevel;
        private List<string> winners;
        private float levelStartTime;
        protected int maxPlayers;

        protected static uint idCounter;

        public override void Connect(int maxPlayers)
        {
            this.maxPlayers = maxPlayers;
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Blocking = false;
            serverSocket.Bind(new IPEndPoint(IPAddress.Any, port));
            serverSocket.Listen(5);
        }

        public override void SendTo(uint user, BasePacket packet)
        {
            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);
            packet.Serialize(bw);
            var data = ms.ToArray();

            connectionInfos.FirstOrDefault(x => x.connectionId == user)?.socket.Send(data);
        }

        public override void SendToAll(BasePacket packet)
        {
            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);
            packet.Serialize(bw);
            var data = ms.ToArray();

            foreach (ConnectionInfo connection in connectionInfos)
                connection.socket.Send(data);
        }

        public override void SendToAllExcept(uint user, BasePacket packet)
        {
            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);
            packet.Serialize(bw);
            var data = ms.ToArray();

            foreach (ConnectionInfo connection in connectionInfos)
                if (connection.connectionId != user)
                    connection.socket.Send(data);
        }

        public override void Disconnect()
        {
            foreach (var connection in connectionInfos)
                connection.socket.Close();
            serverSocket?.Close();
            connectionInfos.Clear();
        }

        public override void Update()
        {
            try
            {
                ConnectionInfo ci = new ConnectionInfo
                {
                    socket = serverSocket.Accept(),
                    connectionId = idCounter++
                };
                connectionInfos.Add(ci);
                InvokePlayerConnected(ci.connectionId);
                Debug.Log("[Server] Local client connected");
            }
            catch (Exception e)
            {
                if (e is not SocketException)
                    Debug.LogError(e);
            }

            try
            {
                for (int i = 0; i < connectionInfos.Count; i++)
                {
                    while (connectionInfos[i].socket.Available > 0)
                    {
                        byte[] buffer = new byte[connectionInfos[i].socket.Available];
                        connectionInfos[i].socket.Receive(buffer);

                        var rms = new MemoryStream(buffer);
                        var br = new BinaryReader(rms);

                        while (rms.Position < rms.Length)
                        {
                            var packet = BasePacket.DeserializePacket(br);
                            HandlePacket(connectionInfos[i].connectionId, packet);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error receiving the packet: " + e);
            }
        }


        public class ConnectionInfo
        {
            public Socket socket;
            public uint connectionId;
        }
    }
}
