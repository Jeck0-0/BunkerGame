/*using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Client 
{
    public class OldCrisisManager : MonoBehaviour
    {
        [Header("Crisis Scheduling")] public List<Crisis> CrisisPool = new List<Crisis>();
        public float MinTimeBetweenCrises = 120f;
        public float MaxTimeBetweenCrises = 300f;

        [Tooltip("Number of crises before game ends")]
        public int TotalCrisisCount = 10;

        [Tooltip("Starting bunker integrity")] public int BunkerIntegrity = 10;

        private int crisesLaunched = 0;
        private ActiveCrisisInstance activeCrisis = null;
        private float timeToNextCrisis = 0f;

        // players in session
        private HashSet<string> currentPlayers = new HashSet<string>(StringComparer.Ordinal);

        public event Action<CrisisStartDTO> OnCrisisStarted;
        public event Action<CrisisPublicResultDTO> OnCrisisResolvedPublic; // to all clients (public info)
        public event Action<PlayerResolveDTO> OnPlayerResolved; // Raised once per player (private info)
        public event Action<bool> OnGameEnded;


        private void Awake()
        {
            ScheduleNextCrisisRandomImmediate();
        }

        private void Update()
        {
            // scheduler
            if (activeCrisis == null)
            {
                if (crisesLaunched >= TotalCrisisCount)
                {
                    // game ended due to crises cap
                    OnGameEnded?.Invoke(false);
                    enabled = false;
                    return;
                }

                timeToNextCrisis -= Time.deltaTime;
                if (timeToNextCrisis <= 0f)
                {
                    TryStartRandomCrisis();
                }
            }
            else
            {
                // active crisis ticking
                if (Time.time >= activeCrisis.EndTime)
                {
                    ResolveActiveCrisis();
                }
            }
        }

        public void SetCurrentPlayers(IEnumerable<string> playerIds)
        {
            currentPlayers = new HashSet<string>(playerIds ?? Enumerable.Empty<string>(), StringComparer.Ordinal);
        }

        public void StartSchedulerNow()
        {
            if (activeCrisis == null)
            {
                ScheduleNextCrisisRandomImmediate();
            }
        }

        #region Commiting Resoources

        public bool TryCommitResources(PlayerContribution contribution)
        {
            if (activeCrisis == null) return false;
            if (!activeCrisis.IsOpen) return false;

            if (!currentPlayers.Contains(contribution.PlayerId))
                return false;

            foreach (var rr in contribution.CommittedResources)
            {
                if (rr.Amount.Any(x=>x.Value < 0)) return false;
            }

            // store commit
            // usses actual current world time (for networking)
            contribution.CommitTimeUtc = DateTime.UtcNow.Subtract(DateTime.UnixEpoch).TotalSeconds;
            activeCrisis.Contributions[contribution.PlayerId] = contribution;

            // if all players committed
            if (activeCrisis.Contributions.Count >= currentPlayers.Count && currentPlayers.Count > 0)
            {
                ResolveActiveCrisis();
            }

            return true;
        }

        #endregion

        #region Scheduling Helpers

        private void ScheduleNextCrisisRandomImmediate()
        {
            float min = Mathf.Max(120f, MinTimeBetweenCrises);
            float max = Mathf.Max(min, MaxTimeBetweenCrises);
            timeToNextCrisis = UnityEngine.Random.Range(min, max);
        }

        private void TryStartRandomCrisis()
        {
            if (activeCrisis != null) return;
            if (CrisisPool == null || CrisisPool.Count == 0) return;
            var chosen = CrisisPool[UnityEngine.Random.Range(0, CrisisPool.Count)];
            StartCrisis(chosen);
        }

        private void StartCrisis(Crisis crisis)
        {
            if (crisis == null) return;
            activeCrisis = new ActiveCrisisInstance(crisis, Time.time, crisis.TimeToResolve);
            crisesLaunched++;

            var dto = new CrisisStartDTO()
            {
                CrisisId = crisis.CrisisId,
                // usses actual current world time (for networking)
                CrisisEndTimeUtc = DateTime.UtcNow.AddSeconds(crisis.TimeToResolve).Subtract(DateTime.UnixEpoch)
                    .TotalSeconds,
            };
            OnCrisisStarted?.Invoke(dto);
        }

        #endregion

        #region Resolution

        private string ResolveTies(List<string> tiedPlayers)
        {
            if (tiedPlayers == null || tiedPlayers.Count == 0)
                return null;

            int index = UnityEngine.Random.Range(0, tiedPlayers.Count);
            return tiedPlayers[index];
        }

        private void ResolveActiveCrisis()
        {
            if (activeCrisis == null) return;

            var crisis = activeCrisis.CrisisDef;
            // success points
            var accepted = new HashSet<ResourceType>(crisis.RequiredResources);

            var perPlayerSuccessPoints = new Dictionary<string, int>(StringComparer.Ordinal);
            var perPlayerTotals = new Dictionary<string, int>(StringComparer.Ordinal);

            foreach (var player in currentPlayers)
            {
                perPlayerSuccessPoints[player] = 0;
                perPlayerTotals[player] = 0;
            }

            foreach (var playerContribution in activeCrisis.Contributions)
            {
                var playerId = playerContribution.Key;
                var contribution = playerContribution.Value;
                int totalUnits = 0;
                int successUnits = 0;

                for (int i = 0; i < contribution.CommittedResources.Count; i++)
                {
                    var resourceAmount = contribution.CommittedResources[i];
                    totalUnits += Mathf.Max(0, resourceAmount.Amount);
                    if (accepted.Contains(resourceAmount.Type))
                        successUnits += Mathf.Max(0, resourceAmount.Amount);
                }

                perPlayerTotals[playerId] = totalUnits;
                perPlayerSuccessPoints[playerId] = successUnits;
            }

            int totalSuccessPoints = perPlayerSuccessPoints.Values.Sum();
            bool globalSuccess = totalSuccessPoints >= crisis.SuccessPointsRequiredPerPlayer * currentPlayers.Count;

            // highest bidder (ties dealt randomly for now)
            string highestBidder = null;
            {
                int best = perPlayerTotals.Values.Max();
                var tied = perPlayerTotals.Where(p => p.Value == best && p.Value > 0).Select(p => p.Key).ToList();
                highestBidder = ResolveTies(tied);
            }

            // lowest bidder (ties dealt randomly for now)
            string lowestBidder = null;
            {
                int worst = perPlayerTotals.Values.Min();
                var tied = perPlayerTotals.Where(p => p.Value == worst).Select(p => p.Key).ToList();
                lowestBidder = ResolveTies(tied);
            }

            // per-player resource changes
            var perPlayerChanges = new Dictionary<string, List<ResourceAmount>>(StringComparer.Ordinal);
            foreach (var player in currentPlayers)
                perPlayerChanges[player] = new List<ResourceAmount>();

            if (globalSuccess)
            {
                // highest bidder gets reward
                if (!string.IsNullOrEmpty(highestBidder))
                {
                    foreach (var r in crisis.HighestBidderReward)
                        perPlayerChanges[highestBidder].Add(r);
                }
            }
            else
            {
                // fail: bunker damaged, penalties to everyone
                BunkerIntegrity = Mathf.Max(0, BunkerIntegrity - crisis.BunkerDamageOnFail);

                foreach (var player in currentPlayers)
                {
                    foreach (var r in crisis.FailurePenalty)
                        perPlayerChanges[player].Add(r);
                }

                // lowest bidder gets penalty 
                if (!string.IsNullOrEmpty(lowestBidder))
                {
                    foreach (var r in crisis.LowestBidderReward)
                        perPlayerChanges[lowestBidder].Add(new ResourceAmount(r.Type, -Mathf.Abs(r.Amount)));
                }
            }

            // apply resource changes
            foreach (var player in currentPlayers)
            {
                // do stuff
            }

            // DTOs (Data Transfer Objects - basically lightweighted data containers)
            PlayerPublicCommitDTO[] publicCommits = null;
            if (!crisis.IsBetHidden)
            {
                publicCommits = currentPlayers.Select(player => new PlayerPublicCommitDTO
                {
                    PlayerId = player,
                    TotalCommitted = perPlayerTotals.ContainsKey(player) ? perPlayerTotals[player] : 0,
                    SuccessPoints = perPlayerSuccessPoints.ContainsKey(player) ? perPlayerSuccessPoints[player] : 0
                }).ToArray();
            }
            else
            {
                publicCommits = new PlayerPublicCommitDTO[0];
            }

            var publicDto = new CrisisPublicResultDTO
            {
                CrisisId = crisis.CrisisId,
                Success = globalSuccess,
                HighestBidderId = highestBidder ?? string.Empty,
                LowestBidderId = lowestBidder ?? string.Empty,
                TotalSuccessPoints = totalSuccessPoints,
                BunkerIntegrityAfter = BunkerIntegrity,
                PublicCommits = publicCommits
            };
            OnCrisisResolvedPublic?.Invoke(publicDto);

            // per-player private DTO
            foreach (var player in currentPlayers)
            {
                var playerDto = new PlayerResolveDTO
                {
                    PlayerId = player,
                    GlobalSuccess = globalSuccess,
                    PlayerTotalCommitted = perPlayerTotals.ContainsKey(player) ? perPlayerTotals[player] : 0,
                    PlayerSuccessPoints =
                        perPlayerSuccessPoints.ContainsKey(player) ? perPlayerSuccessPoints[player] : 0,
                    WasHighestBidder = player == highestBidder,
                    WasLowestBidder = player == lowestBidder,
                    ResourceDeltas = perPlayerChanges[player].ToArray()
                };
                OnPlayerResolved?.Invoke(playerDto);
            }

            // check if bunker destroyed
            if (BunkerIntegrity <= 0)
            {
                OnGameEnded?.Invoke(true);
                enabled = false;
                activeCrisis = null;
                return;
            }

            activeCrisis = null;
            ScheduleNextCrisisRandomImmediate();
        }

        #endregion

        private class ActiveCrisisInstance
        {
            public Crisis CrisisDef;
            public double StartTimeUtc;
            public float StartTimeUnity;
            public float EndTime;

            public Dictionary<string, PlayerContribution> Contributions =
                new Dictionary<string, PlayerContribution>(StringComparer.Ordinal);

            public bool IsOpen => Time.time < EndTime;

            public ActiveCrisisInstance(Crisis def, float startTimeUnity, float duration)
            {
                CrisisDef = def;
                StartTimeUnity = startTimeUnity;
                // usses actual current world time (for networking)
                StartTimeUtc = DateTime.UtcNow.Subtract(DateTime.UnixEpoch).TotalSeconds;
                EndTime = startTimeUnity + duration;
            }
        }
    }

    [Serializable]
    public class PlayerContribution
    {
        public string PlayerId;
        public List<ResourceAmount> CommittedResources = new List<ResourceAmount>();
        public double CommitTimeUtc;

        public PlayerContribution(string playerId)
        {
            PlayerId = playerId;
            CommitTimeUtc = DateTime.UtcNow.Subtract(DateTime.UnixEpoch).TotalSeconds;
        }

        public int TotalCommittedUnits()
        {
            int sum = 0;
            for (int i = 0; i < CommittedResources.Count; i++)
                sum += Mathf.Max(0, CommittedResources[i].Amount);
            return sum;
        }
    }
}*/