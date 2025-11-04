
using System.Collections.Generic;
using Networking;
using UnityEngine;

namespace Client 
{
    public class PlayerManager : Singleton<PlayerManager>
    {
        public Dictionary<uint, Player> Players = new();

        protected override void Awake()
        {
            base.Awake();
        }


        public class Player
        {
            

            Player()
            {
            }


            
            
        }
    }
}
