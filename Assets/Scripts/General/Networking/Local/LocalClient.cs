using System;
using System.IO;
using UnityEngine;
using System.Net.Sockets;

namespace Networking
{
    [CreateAssetMenu(menuName = "ScriptableVariables/Networking/LocalClient", fileName = "LocalClient")]
    public class LocalClient : Client
    {
        private Socket socket;
        private int port = 6969;
        public bool connected = false;

        public override void Connect()
        {
            if (connected)
                return;

            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                socket.Connect("127.0.0.1", port);
                connected = true;
                Debug.Log("[Client] Connected to local server");
            }
            catch (Exception e)
            {
                Debug.LogError("[Client] Failed to connect to local server: " + e.Message);
            }
        }

        public override void Send(BasePacket packet)
        {
            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);
            packet.Serialize(bw);
            byte[] data = ms.ToArray();

            socket.Send(data);
        }

        public override void Disconnect()
        {
            connected = false;
            if(socket != null)
                socket.Close();
        }

        public override void Update()
        {
            if (!connected)
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
    }
}