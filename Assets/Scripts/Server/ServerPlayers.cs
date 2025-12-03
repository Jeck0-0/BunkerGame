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

        protected List<int> occupiedSpots = new ();
        public event Action<Player> OnPlayerQuit;
        
        protected override void Awake()
        {
            base.Awake();
            GameServer.OnPlayerConnected += PlayerConnected;
            GameServer.OnPlayerDisconnected += PlayerDisconnected;
            GameServer.Subscribe<CTS_PlayerInformation>(GetPlayerInformation);
        }


        private void OnDestroy()
        {
            GameServer.OnPlayerConnected -= PlayerConnected;
            GameServer.OnPlayerDisconnected -= PlayerDisconnected;
            GameServer.Unsubscribe<CTS_PlayerInformation>(GetPlayerInformation);
        }
        
        protected void PlayerConnected(uint id)
        {
            Debug.Log("[ServerPlayers] Player connected: " + id);
            if (players.ContainsKey(id))
                Debug.LogError("Duplicate player ID: " + id);
            
            players[id] = new Player(id);
            
            //find first free spot
            int i = 0;
            while (i < 100 && occupiedSpots.Contains(i)) i++;
            players[id].spot = i;
            occupiedSpots.Add(i);
            
            GameServer.SendTo(id, new STC_JoinResponse(id, players[id].spot));

            foreach (var player in GetAll())
                if(player.id != id)
                    GameServer.SendTo(id, new STC_PlayerJoined(player.id, player.username, player.spot, player.emblemData));
            
            //TODO ?disconnect if doesn't send PlayerInformation within 2 seconds
        }
        protected void PlayerDisconnected(uint id)
        {
            Debug.Log("Player left: " + id);
            OnPlayerQuit?.Invoke(Get(id));
            occupiedSpots.Remove(players[id].spot);
            players.Remove(id);
        }

        private void GetPlayerInformation(uint id, BasePacket p)
        {
            CTS_PlayerInformation playerInfo = (CTS_PlayerInformation)p;
            players[id].emblemData = playerInfo.emblemData;
            players[id].username = playerInfo.username;
            GameServer.SendToAllExcept(id, new STC_PlayerJoined(id, players[id].username, players[id].spot, players[id].emblemData));
        }
        
        

        public class Player
        {
            public uint id;
            public int spot;
            public string username = string.Empty;
            public EmblemData emblemData = new EmblemData();
            public PlayerResources resources = new PlayerResources();
            public SecretObjective SecretObjective;
            public int VP;

            public Player(uint id)
            {
                this.id = id;
            }
        }
    }
}