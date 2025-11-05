using System;
using System.Collections.Generic;
using Networking;
using UnityEngine;

namespace Server
{
    public class ServerPlayers : Singleton<ServerPlayers>
    {
        protected Dictionary<uint, Player> players = new();

        public static Player Get(uint id) => Instance.players[id];

        public event Action<Player> OnPlayerQuit;
        
        protected override void Awake()
        {
            base.Awake();
            NetworkManager.Server.OnPlayerConnected += PlayerConnected;
            NetworkManager.Server.OnPlayerDisconnected += PlayerDisconnected;
            NetworkManager.Server.Subscribe<CTS_PlayerInformation>(GetPlayerInformation);
        }


        private void OnDestroy()
        {
            NetworkManager.Server.OnPlayerConnected -= PlayerConnected;
            NetworkManager.Server.OnPlayerDisconnected -= PlayerDisconnected;
            NetworkManager.Server.Unsubscribe<CTS_PlayerInformation>(GetPlayerInformation);
        }
        
        protected void PlayerConnected(uint id)
        {
            if (players.ContainsKey(id))
                Debug.LogError("Duplicate player ID: " + id);
            
            players[id] = new Player(id);
            
            //TODO ?disconnect if doesn't send PlayerInformation within 2 seconds
        }
        protected void PlayerDisconnected(uint id)
        {
            Debug.Log("Player left: " + id);
            OnPlayerQuit?.Invoke(Get(id));
            players.Remove(id);
        }

        private void GetPlayerInformation(uint playerId, BasePacket p)
        {
            CTS_PlayerInformation playerInfo = (CTS_PlayerInformation)p;
            players[playerId].emblemData = playerInfo.emblemData;
            players[playerId].username = playerInfo.username;
        }
        
        

        public class Player
        {
            public uint id;
            public string username;
            public EmblemData emblemData;
            //resources

            public Player(uint id)
            {
                this.id = id;
            }
        }
    }
}