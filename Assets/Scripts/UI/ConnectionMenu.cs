using UnityEngine;

public class ConnectionMenu : MonoBehaviour
{
    public void JoinGame()
    {
        NetworkManager.Instance.StartClient();
        //SteamLobby.OpenJoinOverlay();
    }

    public void HostGame()
    {
        NetworkManager.Instance.StartServerAndClient();
        //SteamLobby.OpenInviteOverlay();
    }
}