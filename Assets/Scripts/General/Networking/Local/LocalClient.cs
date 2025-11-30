using System;
using System.IO;
using Networking;
using UnityEngine;
using System.Net.Sockets;

namespace Networking
{
    public class LocalClient : GameClient
    {
        private Socket socket;
        private int port = 6969;
        
        protected override bool isConnected {
            get => socket.Connected;
            set { } 
        }

        protected override void Awake()
        {
            base.Awake();
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        private void OnApplicationQuit()
        {
            Disconnect();
        }

        void Update()
        {
            if (!isConnected)
                return;

            while (socket.Available > 0)
            {
                byte[] receivedBuffer = new byte[socket.Available];
                socket.Receive(receivedBuffer);

                using var rms = new MemoryStream(receivedBuffer);
                using var br = new BinaryReader(rms);

                while (rms.Position < rms.Length)
                {
                    var packet = BasePacket.DeserializePacket(br);
                    HandlePacket(packet);
                }
            }
        }


        public override void Connect(object args)
        {
            if (isConnected)
                return;

            var connectToIp = "127.0.0.1";
            var connectToPort = port;
            
            if (args is (string ip, int prt))
            {
                connectToIp = ip;
                connectToPort = prt;
            }
            
            
            try
            {
                socket.Connect(connectToIp, connectToPort);
                isConnected = true;
                Debug.Log("[Client] Connected to local server");
            }
            catch (Exception e)
            {
                Debug.LogError("[Client] Failed to connect to local server: " + e.Message);
            }
        }

        public override void Disconnect()
        {
            isConnected = false;
            if(socket != null)
                socket.Close();
        }

        protected override void SendLogic(BasePacket packet)
        {
            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);
            packet.Serialize(bw);
            byte[] data = ms.ToArray();

            socket.Send(data);
        }
    }
}