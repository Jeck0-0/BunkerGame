using Networking;
using Packets;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ConnectionMenu : MonoBehaviour
{
    private void Awake()
    {
        SteamClient.Subscribe<STC_JoinResponse>(StartGame);
    }
    private void OnDestroy()
    {
        SteamClient.Unsubscribe<STC_JoinResponse>(StartGame);
    }

    public void JoinGame()
    {
        SteamLobby.OpenJoinOverlay();
    }

    public void HostGame()
    {
        SteamServer.Instance.Create(6);
        SteamLobby.OpenInviteOverlay();
        //StartGame();
    }

    protected void StartGame(object _)
    {
        SceneManager.LoadScene("MeetingRoom");
    }
}