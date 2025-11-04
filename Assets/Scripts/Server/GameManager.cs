using System.Collections.Generic;
using Networking;

namespace Server
{
    public class GameManager : Singleton<GameManager>
    {
        public Dictionary<uint, PlayerInfo> players = new ();
        public static Dictionary<uint, PlayerInfo> Players => Instance.players;

        protected override void Awake()
        {
            base.Awake();
        }

        public class PlayerInfo
        {
            public TrackAmount Tracks;
            
        }
    }
}