using Networking;
using Packets;
using System.Linq;
using UnityEngine;

namespace Server
{
    public class SecretObjectiveManager : Singleton<SecretObjectiveManager>
    {
        [SerializeField] SecretObjective[] essentialObjectives;
        [SerializeField] SecretObjective[] randomObjectives;

        public void SetSecretObjectives()
        {
            var players = ServerPlayers.GetAll().ToList();

            if (essentialObjectives.Length + randomObjectives.Length < players.Count)
            {
                Debug.LogError("Not enough secret objectives for all players");
                return;
            }

            var shuffledPlayers = players.OrderBy(x => Random.value).ToList();
            var essentialPool = essentialObjectives.OrderBy(x => Random.value).ToList();
            var randomPool = randomObjectives.OrderBy(x => Random.value).ToList();

            int playerIndex = 0;

            foreach (var essential in essentialPool)
            {
                if (playerIndex >= shuffledPlayers.Count) break;

                shuffledPlayers[playerIndex].SecretObjective = essential;
                GameServer.SendTo(shuffledPlayers[playerIndex].id, new STC_SecretObjective(essential));
                playerIndex++;
            }

            for (int i = 0; i < players.Count - essentialObjectives.Length; i++)
            {
                shuffledPlayers[playerIndex].SecretObjective = randomPool[i];
                GameServer.SendTo(shuffledPlayers[playerIndex].id, new STC_SecretObjective(randomPool[i]));
                playerIndex++;
            }

            Debug.Log("All secret objectives assigned");
            PhaseManager.Instance.GameStart();
        }
    }
}