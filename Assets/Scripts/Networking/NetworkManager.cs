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
        
        client.Connect();
    }

    public void StopServer()
    {
        server.Disconnect();
        serverRunning = false;
    }
    
    public void StartServer()
    {
        server.Connect(5);
        serverRunning = true;
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