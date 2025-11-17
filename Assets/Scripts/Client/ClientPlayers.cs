using System;
using Packets;
using System.Collections.Generic;
using System.Linq;
using Networking;
using Sirenix.OdinInspector;

namespace Client 
{
    public class ClientPlayers : PersistentSingleton<ClientPlayers>
    {
        [ShowInInspector, ReadOnly] protected Dictionary<uint, Player> Players = new();
        [ShowInInspector, ReadOnly] protected Player myself;
        public event Action OnSpotReceived;
        
        public Player Get(uint playerId) => Players[playerId];
        public IEnumerable<Player> GetOthers() => Players.Values;
        public IEnumerable<Player> GetAll() => Players.Values.Append(myself);
        public Player Myself => myself;
        
        protected override void Awake()
        {
            base.Awake();
            NetworkManager.Client.Subscribe<STC_PlayerJoined>(OnPlayerJoined);
            NetworkManager.Client.Subscribe<STC_JoinResponse>(OnJoinResponse);
        }

        private void OnDestroy()
        {
            NetworkManager.Client?.Unsubscribe<STC_PlayerJoined>(OnPlayerJoined);
            NetworkManager.Client?.Unsubscribe<STC_JoinResponse>(OnJoinResponse);
        }

        private void OnJoinResponse(BasePacket p)
        {
            STC_JoinResponse packet = p as STC_JoinResponse;
            myself = new Player(999);
            myself.spot = packet.spot;
            OnSpotReceived?.Invoke();
        }
        private void OnPlayerJoined(BasePacket p)
        {
            STC_PlayerJoined packet = p as STC_PlayerJoined;
            Players.Add(packet.playerId, new Player(packet.playerId));
            Players[packet.playerId].emblemData = packet.emblemData;
            Players[packet.playerId].username = packet.username;
        }


        public class Player
        {
            public uint id;
            public string username;
            public int spot;
            public EmblemData emblemData;

            public Player(uint id)
            {
                this.id = id;
            }
        }
    }
}
