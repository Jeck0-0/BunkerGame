using UnityEngine;

namespace Server
{
    public class SecretObjectiveManager : Singleton<SecretObjectiveManager>
    {
        [SerializeField] SecretObjective[] objectives;

        public void SetSecretObjectives()
        {
            var players = ServerPlayers.GetAll();
        }
    }
}