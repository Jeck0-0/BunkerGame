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

            // Positive tracks
            foreach (TrackType track in obj.PositiveTracks)
            {
                if (ServerTracks.Instance.GetTrackValue(track, out int posValue))
                {
                    total += GetVPFromTable(posValue, obj.PositiveTable);
                }
            }

            // Negative tracks
            foreach (TrackType track in obj.NegativeTracks)
            {
                if (ServerTracks.Instance.GetTrackValue(track, out int posValue))
                {
                    total += GetVPFromTable(posValue, obj.NegativeTable);
                }
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