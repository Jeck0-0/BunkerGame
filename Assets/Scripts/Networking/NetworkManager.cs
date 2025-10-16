using System;
using UnityEngine;

public class NetworkManager : Singleton<NetworkManager>
{
    public Client client;
    public Server server;
    
    bool serverRunning = false;

    protected override void Awake()
    {
        base.Awake();
        if (client == null)
        {
            Debug.LogWarning("No client assigned to network manager", this);
            return;
        }
    }

    public void StopServerAndClient()
    {
        client.Disconnect();
        StopServer();
    }
    public void StopServer()
    {
        server.Disconnect();
        serverRunning = false;
    }

    public void StartClient()
    {
        client.Connect();
    }
    public void StartServerAndClient()
    {
        server.Connect(5);
        serverRunning = true;
        client.Connect();
    }
    
    private void Update()
    {
        client.Update();
        if(serverRunning)
            server.Update();
    }

    private void OnDestroy()
    {
        client.Disconnect();
        server.Disconnect();
    }
}