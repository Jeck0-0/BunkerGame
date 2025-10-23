
using System.Collections.Generic;

namespace Client 
{
    public class PlayerManager : Singleton<PlayerManager>
    {
        public Dictionary<uint, Player> Players = new();
        
        
        


        public class Player
        {
            public ResourceAmount resources;

            Player()
            {
            }
        }
    }
}
