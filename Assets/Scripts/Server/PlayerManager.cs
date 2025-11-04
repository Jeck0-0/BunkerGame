using System;
using System.Collections.Generic;
using Networking;
using UnityEngine;

namespace Server
{
    public class PlayerManager : Singleton<PlayerManager>
    {
        protected Dictionary<uint, Player> players = new();

        public static Player Get(uint id) => Instance.players[id];

        public event Action<Player> OnPlayerQuit;
        
        protected override void Awake()
        {
            base.Awake();
            NetworkManager.Server.OnPlayerConnected += PlayerConnected;
            NetworkManager.Server.OnPlayerDisconnected += PlayerDisconnected;
        }

        private void OnDestroy()
        {
            NetworkManager.Server.OnPlayerConnected -= PlayerConnected;
            NetworkManager.Server.OnPlayerDisconnected -= PlayerDisconnected;
        }
        
        protected void PlayerDisconnected(uint id)
        {
            Debug.Log("Player left: " + id);
            OnPlayerQuit?.Invoke(Get(id));
            players.Remove(id);
        }
        protected void PlayerConnected(uint id)
        {
            if (players.ContainsKey(id))
                Debug.LogError("Duplicate player ID: " + id);
            
            players[id] = new Player(id);
        }

        public class Player
        {
            public uint id;
            //faction info
            //resources

            public Player(uint id)
            {
                this.id = id;
            }
        }
    }
}