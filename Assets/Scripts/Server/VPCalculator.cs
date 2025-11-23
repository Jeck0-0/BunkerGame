using System.Collections.Generic;
using UnityEngine;

namespace Server
{
    public class VPCalculator : Singleton<VPCalculator>
    {
        public int CalculatePlayerVP(ServerPlayers.Player player)
        {
            if (player.SecretObjective == null)
            {
                Debug.LogError($"Player {player.id} has no secret objective");
                return 0;
            }

            var obj = player.SecretObjective;
            int total = 0;

            // Positive track
            if (ServerTracks.Instance.GetTrackValue(obj.PositiveTrack, out int posValue))
            {
                total += GetVPFromTable(posValue, obj.PositiveTable);
            }

            // Negative track
            if (ServerTracks.Instance.GetTrackValue(obj.NegativeTrack, out int negValue))
            {
                total += GetVPFromTable(negValue, obj.NegativeTable);
            }

            return total;
        }

        int GetVPFromTable(int level, List<LevelVP> table)
        {
            foreach (var _level in table)
            {
                if (_level.Level == level) 
                    return _level.Points;
            }

            return 0;
        }
    }
}