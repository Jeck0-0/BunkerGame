using Networking;
using Packets;
using System.Linq;
using UnityEngine;

namespace Server
{
    public class SecretObjectiveManager : Singleton<SecretObjectiveManager>
    {
        [SerializeField] SecretObjective[] objectives;

        public void SetSecretObjectives()
        {
            var players = ServerPlayers.GetAll().ToList();

            if (objectives.Length < players.Count)
            {
                Debug.LogError("Not enough secret objectives for all players");
                return;
            }

            // random objectives
            var pool = objectives.OrderBy(x => Random.value).ToList();

            for (int i = 0; i < players.Count; i++)
            {
                players[i].SecretObjective = pool[i];

                // Send objective
                NetworkManager.Server.SendTo(players[i].id, new STC_SecretObjective(pool[i]));
            }

            Debug.Log("All secret objectives assigned");
            PhaseManager.Instance.GameStart();
        }
    }
}