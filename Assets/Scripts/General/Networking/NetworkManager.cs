using UnityEngine;

namespace Networking
{
    public class NetworkManager : Singleton<NetworkManager>
    {
        [SerializeField] protected Client client;
        [SerializeField] protected Server server;

        public static Server Server => Instance?.server;
        public static Client Client => Instance?.client;

        bool serverRunning = false;

        protected override void Awake()
        {
            base.Awake();
            if (Client == null)
            {
                Debug.LogWarning("No client assigned to network manager", this);
                return;
            }
        }

        public void StopServerAndClient()
        {
            Client.Disconnect();
            StopServer();
        }

        public void StopServer()
        {
            Server.Disconnect();
            serverRunning = false;
        }

        public void StartClient()
        {
            Client.Connect();
        }

        public void StartServerAndClient()
        {
            Server.Connect(5);
            serverRunning = true;
            Client.Connect();
        }

        private void Update()
        {
            Client.Update();
            if (serverRunning)
                Server.Update();
        }

        private void OnDestroy()
        {
            if (Client) Client.Disconnect();
            if (Server) Server.Disconnect();
        }
    }
}