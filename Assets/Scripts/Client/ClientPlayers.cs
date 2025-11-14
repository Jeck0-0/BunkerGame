using Packets;
using System.Collections.Generic;
using Networking;
using Sirenix.OdinInspector;

namespace Client 
{
    public class ClientPlayers : PersistentSingleton<ClientPlayers>
    {
        [ShowInInspector, ReadOnly] Dictionary<uint, Player> Players = new();

        public Player Get(uint playerId) => Players[playerId];
        public IEnumerable<Player> GetAll() => Players.Values;
        
        protected override void Awake()
        {
            base.Awake();
            NetworkManager.Client.Subscribe<STC_PlayerJoined>(OnPlayerJoined);
        }

        private void OnDestroy()
        {
            NetworkManager.Client?.Unsubscribe<STC_PlayerJoined>(OnPlayerJoined);
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
            public EmblemData emblemData;

            public Player(uint id)
            {
                this.id = id;
            }
        }
    }
}
