using UnityEngine;
using System.Linq;
using Server;
using Client;
using Packets;
using Networking;
public class PlayerSpawner : MonoBehaviour
{
    [SerializeField]private GameObject PlayerObj;
    [SerializeField] private Transform[] seats;
    [SerializeField] private Transform RMap;

    public int MaxPlayers = 6;

    private int mySpot = 0;
    private bool KnowMe = false;
    void Awake()
    {
        NetworkManager.Client.Subscribe<STC_PlayerJoined>(NewPlayerSpawned);
        NetworkManager.Client.Subscribe<STC_JoinResponse>(SpawnMe);
        Debug.LogError("subscribed");
    }

    private void OnDestroy()
    {
        try
        {
        NetworkManager.Client.Unsubscribe<STC_PlayerJoined>(NewPlayerSpawned);
        NetworkManager.Client.Unsubscribe<STC_JoinResponse>(SpawnMe);
        }
        catch
        {}
    }
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.S))
        {
            
        }
    }

   private void SpawnMe(BasePacket p)
    {
        Debug.LogError("Spawnme");
        STC_JoinResponse playerInfo = (STC_JoinResponse)p;

        mySpot = playerInfo.spot;
        KnowMe = true;

        float MapRotation = (mySpot - 1) * 60f;

        RMap.localRotation = Quaternion.Euler(0f, MapRotation, 0f);

        SpawnExistingPlayers();
    }

    private void SpawnExistingPlayers()
    {
        ClientPlayers playerRegistry = ClientPlayers.Instance;

        foreach (ClientPlayers.Player other in playerRegistry.GetOthers())
        {
            int otherSpot = other.spot;    // their player number
            SpawnAtSeat(otherSpot);
        }
    }

    private void NewPlayerSpawned(BasePacket p)
    {
        Debug.LogError("spawning new player");
        if (!KnowMe)
        {
            Debug.LogError("trying to spawn before I know myself.");
            return;
        }
        STC_PlayerJoined playerInfo = (STC_PlayerJoined)p;

        int otherSpot = playerInfo.spot;

        SpawnAtSeat(otherSpot);

    }

    private void SpawnAtSeat(int otherSpot)
    {
       
        int Pcount = ClientPlayers.Instance.GetAll().Count();

        if (Pcount >= MaxPlayers) 
        {
            Debug.LogError($"max players reached {Pcount} out of {MaxPlayers}.");
            return;
        }
        
        int Seat = ((otherSpot - mySpot) + MaxPlayers) % MaxPlayers;

        if (Seat < 0 || Seat >= seats.Length)
        {
            Debug.LogError($"Seat index {Seat} out of range.");
            return;
        }

        Transform seat = seats[Seat];
        Instantiate(PlayerObj, seat.position, seat.rotation);

        Debug.LogError($"spawning {otherSpot} at {Seat}");
    }
}
