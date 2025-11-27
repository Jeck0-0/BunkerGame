using Client;
using Networking;
using Packets;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    private ClientPlayers playerRegistry = ClientPlayers.Instance;
    [SerializeField] private TMP_Text[] PlayerName;

    void Awake()
    {
        SteamClient.Subscribe<STC_PlayerJoined>(AddNewPlayer);
    }

    private void Start()
    {
        AddMe();
        AddPlayers();
    }

    void Update()
    {

    }

    void AddMe()
    {
        int mySpot = playerRegistry.Myself.spot;
        string myName = playerRegistry.Myself.username;

        PlaceInLobby(mySpot, myName);
    }

    void AddPlayers()
    {
        foreach (ClientPlayers.Player other in playerRegistry.GetOthers())
        {
            int otherSpot = other.spot;
            string playerName = other.username;
            PlaceInLobby(otherSpot, playerName);
        }
    }

    void AddNewPlayer(BasePacket p)
    {
        STC_PlayerJoined playerInfo = (STC_PlayerJoined)p;
        int otherSpot = playerInfo.spot;
        string playerName = playerInfo.username;
        PlaceInLobby(otherSpot, playerName);
    }

    void PlaceInLobby(int spot, string playerName)
    {
        PlayerName[spot].text = playerName + (spot + 1);
    }
}

