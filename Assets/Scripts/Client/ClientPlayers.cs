using Networking;
using Packets;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;

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
            GameClient.Subscribe<STC_PlayerJoined>(OnPlayerJoined);
            GameClient.Subscribe<STC_JoinResponse>(OnJoinResponse);
            GameClient.Subscribe<STC_PlayerDisconected>(OnPlayerDisconected);
        }

        private void OnDestroy()
        {
            GameClient.Unsubscribe<STC_PlayerJoined>(OnPlayerJoined);
            GameClient.Unsubscribe<STC_JoinResponse>(OnJoinResponse);
            GameClient.Unsubscribe<STC_PlayerDisconected>(OnPlayerDisconected);
        }

        private void OnJoinResponse(BasePacket p)
        {
            STC_JoinResponse packet = p as STC_JoinResponse;
            myself = new Player(999);
            myself.spot = packet.spot;
            OnSpotReceived?.Invoke();
            
            //Send my info
            CTS_PlayerInformation myInfo = new CTS_PlayerInformation();
            myInfo.username = Steamworks.SteamClient.IsValid ? Steamworks.SteamClient.Name : Environment.UserName;
            myInfo.emblemData = new EmblemData();
            GameClient.Send(myInfo);
        }
        
        private void OnPlayerJoined(BasePacket p)
        {
            STC_PlayerJoined packet = p as STC_PlayerJoined;
            Players.Add(packet.playerId, new Player(packet.playerId));
            Players[packet.playerId].emblemData = packet.emblemData;
            Players[packet.playerId].username = packet.username;
            Players[packet.playerId].spot = packet.spot;
        }
        private void OnPlayerDisconected(BasePacket p)
        {
            STC_PlayerDisconected packet = (STC_PlayerDisconected)p;

            int spot = packet.spot;

            var toRemove = Players.FirstOrDefault(x => x.Value.spot == spot);
            if (toRemove.Value != null)
            {
                Players.Remove(toRemove.Key);
                PlayerManager.Instance.RemovePlayerObject(spot);
            }
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
