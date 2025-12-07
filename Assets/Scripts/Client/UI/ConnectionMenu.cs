using Client;
using Networking;
using Packets;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ConnectionMenu : MonoBehaviour
{
    [SerializeField] Transform LobbyParent;
    [SerializeField] GameObject hostLobby;
    [SerializeField] GameObject clientLobby;

    public string loadScene = "MeetingRoom";
    
    private GameObject conectionInstance;

    private void Awake()
    {
        GameClient.Subscribe<STC_GameStart>(StartGame);
        ClientPlayers.Instance.OnSpotReceived += StartClientLobby;
    }
    private void OnDestroy()
    {
        GameClient.Unsubscribe<STC_GameStart>(StartGame);
        ClientPlayers.Instance.OnSpotReceived -= StartClientLobby;
    }

    public void HostGame() // server is created by host lobby manager
    {
        ComputerUI.Instance.Lobby();
        conectionInstance = Instantiate(hostLobby, LobbyParent);
    }

    public void StartClientLobby()
    {
        if (conectionInstance != null) return; // means that you are hosting already

        ComputerUI.Instance.Lobby();
        GameClient.Instance.Connect(null);
        conectionInstance = Instantiate(clientLobby, LobbyParent);
    }

    protected void StartGame(object _)
    {
        SceneManager.LoadScene(loadScene);
    }
}