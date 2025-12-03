using Client;
using Networking;
using Packets;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;
    [SerializeField] private GameObject PlayerObj;
    [SerializeField] private Transform[] seats;
    [SerializeField] private Transform RMap;

    public int MaxPlayers = 6;
    private ClientPlayers playerRegistry => ClientPlayers.Instance;
    private int mySpot = 0;
    private bool KnowMe = false;

    public Dictionary<int, PlayerInstance> PlayerGNS;

    void Awake()
    {
        Instance = this;

        PlayerGNS = new Dictionary<int, PlayerInstance>();

        GameClient.Subscribe<STC_PlayerJoined>(NewPlayerSpawned);

        if (playerRegistry.Myself != null)
        {
            SpawnMe();
        }
        else
        {
            playerRegistry.OnSpotReceived += SpawnMe;
        }

        /*if (playerRegistry.Myself == null)
        {
            Debug.Log("Error: Myself is still null when entering the scene!");
            return;
        }*/
        //Debug.Log("subscribed");
    }

    private void OnDestroy()
    {
        try
        {
            GameClient.Unsubscribe<STC_PlayerJoined>(NewPlayerSpawned);
            if (ClientPlayers.Instance != null)
            {
                ClientPlayers.Instance.OnSpotReceived -= SpawnMe;
            }

        }
        catch
        { }
    }
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.S))
        { SpawnMe(); }

    }

    private void SpawnMe()
    {
        //Debug.Log($"My spot in ClientPlayers: {playerRegistry.Myself?.spot}");
        //Debug.Log("Spawnme");

        mySpot = playerRegistry.Myself.spot;
        KnowMe = true;

        float MapRotation = (mySpot - 1) * 60f;
        RMap.localRotation = Quaternion.Euler(0f, MapRotation, 0f);

        SpawnExistingPlayers();
    }

    private void SpawnExistingPlayers()
    {
        foreach (ClientPlayers.Player other in playerRegistry.GetOthers())
        {
            int otherSpot = other.spot;
            SpawnAtSeat(otherSpot);
        }
    }

    private void NewPlayerSpawned(BasePacket p)
    {
        //Debug.Log("spawning new player");

        if (!KnowMe)
        {
            Debug.LogWarning("trying to spawn before I know myself.");
            return;
        }

        STC_PlayerJoined playerInfo = (STC_PlayerJoined)p;
        int otherSpot = playerInfo.spot;

        SpawnAtSeat(otherSpot);
    }

    private void SpawnAtSeat(int otherSpot)
    {
        int Pcount = ClientPlayers.Instance.GetAll().Count();

        if (Pcount > MaxPlayers)
        {
            Debug.LogWarning($"max players reached {Pcount} out of {MaxPlayers}.");
            return;
        }

        int Seat = ((otherSpot - mySpot) + MaxPlayers) % MaxPlayers;

        if (Seat < 0 || Seat >= seats.Length)
        {
            Debug.LogWarning($"Seat index {Seat} out of range.");
            return;
        }

        Transform seat = seats[Seat];
        GameObject obj = Instantiate(PlayerObj, seat.position, seat.rotation);

        PlayerInstance instance = new PlayerInstance
        {
            info = ClientPlayers.Instance.GetAll().First(p => p.spot == otherSpot),
            obj = obj
        };
        PlayerGNS.Add(otherSpot, instance);
        //Debug.Log($"spawning {otherSpot} at {Seat}");
    }
    public void RemovePlayerObject(int spot)
    {
        try
        {
        Destroy(PlayerGNS[spot].obj);
        PlayerGNS.Remove(spot);
        }
        catch {}

        Debug.Log($"Destroyed player at spot {spot}");
    }
}
public class PlayerInstance
{
    public ClientPlayers.Player info;
    public GameObject obj;
}