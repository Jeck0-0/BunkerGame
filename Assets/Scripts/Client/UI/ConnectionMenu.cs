using Networking;
using Packets;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ConnectionMenu : MonoBehaviour
{
    public void JoinGame()
    {
        SteamClient.OnConnect += SendMyInfo;
        SteamLobby.OpenJoinOverlay();
    }

    public void HostGame()
    {
        SteamClient.OnConnect += SendMyInfo;
        SteamServer.Instance.Create(6);
        SteamLobby.OpenInviteOverlay();
    }
    public void StartGame()
    {
        SceneManager.LoadScene("MeetingRoom");
    }

    protected void SendMyInfo()
    {
        CTS_PlayerInformation myInfo = new CTS_PlayerInformation();
        myInfo.username = Environment.UserName; // for testing, while we're not using steam
        SteamClient.Send(myInfo);
        SteamClient.OnConnect -= SendMyInfo;
    }
}