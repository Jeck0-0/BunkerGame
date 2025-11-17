using System;
using Packets;
using System.Collections.Generic;
using Networking;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Server
{
    public class ServerPlayers : PersistentSingleton<ServerPlayers>
    {
        [ShowInInspector, ReadOnly] protected Dictionary<uint, Player> players = new();

        public static Player Get(uint id) => Instance.players[id];
        public static IEnumerable<Player> GetAll() => Instance.players.Values;

        protected List<int> occupiedSpots;
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
            if (NetworkManager.Server != null)
            {
                NetworkManager.Server.OnPlayerConnected -= PlayerConnected;
                NetworkManager.Server.OnPlayerDisconnected -= PlayerDisconnected;
                NetworkManager.Server.Unsubscribe<CTS_PlayerInformation>(GetPlayerInformation);
            }
        }
        
        protected void PlayerConnected(uint id)
        {
            if (players.ContainsKey(id))
                Debug.LogError("Duplicate player ID: " + id);
            
            var allPlayers = GetAll();
            
            players[id] = new Player(id);
            
            //find first free spot
            int i = 0;
            for (; i < 100 && occupiedSpots.Contains(i); i++) { }
            players[id].spot = i;
            occupiedSpots.Add(i);
            
            foreach (var player in allPlayers)
                NetworkManager.Server.SendTo(id, new STC_PlayerJoined(player.id, player.username, player.spot, player.emblemData));
            
            NetworkManager.Server.SendTo(id, new STC_JoinResponse(players[id].spot));
            //TODO ?disconnect if doesn't send PlayerInformation within 2 seconds
        }
        protected void PlayerDisconnected(uint id)
        {
            Debug.Log("Player left: " + id);
            OnPlayerQuit?.Invoke(Get(id));
            occupiedSpots.Remove(players[id].spot);
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
            public int spot;
            public string username;
            public EmblemData emblemData;
            public PlayerResources resources = new PlayerResources();

            public Player(uint id)
            {
                this.id = id;
            }
        }
    }
}