using UnityEngine;

public class ConnectionMenu : MonoBehaviour
{
    public void JoinGame()
    {
        SteamLobby.OpenJoinOverlay();
    }

    public void HostGame()
    {
        NetworkManager.Instance.StartServer();
        SteamLobby.OpenInviteOverlay();
    }
}