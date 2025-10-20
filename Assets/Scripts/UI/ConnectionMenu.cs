using UnityEngine;

public class ConnectionMenu : MonoBehaviour
{
    public void JoinGame()
    {
        NetworkManager.Instance.StartClient();
        if(NetworkManager.Client is SteamClient)
            SteamLobby.OpenJoinOverlay();
    }

    public void HostGame()
    {
        NetworkManager.Instance.StartServerAndClient();
        if(NetworkManager.Server is SteamServer)
            SteamLobby.OpenInviteOverlay();
    }
}