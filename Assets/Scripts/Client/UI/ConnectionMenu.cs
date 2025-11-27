using Networking;
using Packets;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ConnectionMenu : MonoBehaviour
{
    private void Awake()
    {
        GameClient.Subscribe<STC_JoinResponse>(StartGame);
    }
    private void OnDestroy()
    {
        GameClient.Unsubscribe<STC_JoinResponse>(StartGame);
    }

    public void JoinGame()
    {
        GameClient.Instance.Connect(null);
    }

    public void HostGame()
    {
        GameServer.Instance.Create(6);
        //StartGame();
    }

    protected void StartGame(object _)
    {
        SceneManager.LoadScene("MeetingRoom");
    }
}