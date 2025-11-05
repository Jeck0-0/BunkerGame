using System;
using UnityEngine;
using Networking;

public class ConnectionMenu : MonoBehaviour
{
    public void JoinGame()
    {
        NetworkManager.Instance.StartClient();
        if(NetworkManager.Client is SteamClient)
            SteamLobby.OpenJoinOverlay();
        SendMyInfo();
    }

    public void HostGame()
    {
        NetworkManager.Instance.StartServerAndClient();
        if(NetworkManager.Server is SteamServer)
            SteamLobby.OpenInviteOverlay();
        SendMyInfo();
    }

    protected void SendMyInfo()
    {
        CTS_PlayerInformation myInfo = new CTS_PlayerInformation();
        myInfo.username = Environment.UserName; // for testing, while we're not using steam
        myInfo.emblemData = new EmblemData();
        NetworkManager.Client.Send(myInfo);
    }
}