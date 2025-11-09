using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Client;
using Networking;
using Packets;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Server
{
    public class CrisisPhase : MonoBehaviour
    {
        public Crisis[] crisisPool;

        protected Dictionary<uint, int> contributions = new ();
        protected Crisis CurrentEmergency;

        private void Awake()
        {
            crisisPool = Resources.LoadAll<Crisis>("ScriptableObjects/Emergencies/Crisis");
        }

        public IEnumerator PlayPhase()
        {
            StartRandomCrisis();
            
            // Wait for all contributions or timer end
            NetworkManager.Server.Subscribe<CTS_ContributeToCrisis>(ReceiveContribution);
            float endTime = Time.time + CurrentEmergency.TimeToResolve;
            yield return new WaitUntil(() => contributions.Count >= ServerPlayers.GetAll().Count() || Time.time > endTime);
            NetworkManager.Server.Unsubscribe<CTS_ContributeToCrisis>(ReceiveContribution);
            
            CalculateCrisisResult();
        }

        protected void StartRandomCrisis()
        {
            // Get random crisis
            if (crisisPool == null || crisisPool.Length == 0) Debug.LogError("No crisis in pool");
            CurrentEmergency = crisisPool[Random.Range(0, crisisPool.Length)];
            
            // Send crisis start packet
            STC_StartEmergency packet = new STC_StartEmergency(CurrentEmergency, DateTime.Now.Ticks);
            NetworkManager.Server.SendToAll(packet);
            
            // Prepare to receive contributions
            contributions.Clear();
        }
        
        protected void ReceiveContribution(uint player, BasePacket packet)
        {
            CTS_ContributeToCrisis contributionPacket = packet as CTS_ContributeToCrisis;
            if (contributions.ContainsKey(player))
            {
                Debug.LogWarning($"Player {player} sent multiple contributions (not allowed smh)");
                return;
            }

            if (ServerPlayers.Get(player) == null)
            {
                Debug.LogWarning($"Received contribution from unknown player {player}");
                return;
            }

            if (ServerPlayers.Get(player).resources.Materials < contributionPacket.materials)  //![player].Tracks.Has(contributionPacket.TrackAmount)) DOESNT HAVE RESOURCES
            {
                Debug.LogWarning($"Player {player} tried to contribute more materials than they have {contributionPacket.materials}");
                contributionPacket.materials = ServerPlayers.Get(player).resources.Materials; // clamp
                return;
            }
            
            contributions[player] = contributionPacket.materials;
            ServerPlayers.Get(player).resources.ModifyMaterials(-contributionPacket.materials);
        }

        protected void CalculateCrisisResult()
        {
            //crisis result
            int totalContributions = 0;
            foreach (var contribution in contributions.Values)
                totalContributions += contribution;

            bool success = totalContributions >= CurrentEmergency.requiredMaterials;

            // highest and lowest bidders
            int highestContribution = contributions.Count > 0 ? contributions.Values.Max() : 0;
            int lowestContribution = contributions.Count > 0 ? contributions.Values.Min() : 0;

            var highestContributors = contributions.Where(kv => kv.Value == highestContribution).Select(kv => kv.Key).ToList();
            var lowestContributors = contributions.Where(kv => kv.Value == lowestContribution).Select(kv => kv.Key).ToList();


            /*int highestContribution = contributions.Values.Max(x=>x.Amount.Values.Sum());
			int lowestContribution = contributions.Values.Min(x=>x.Amount.Values.Sum());

            IEnumerable<uint> highestContributors = contributions
                .Where(x => x.Value.Amount.Values.Sum() == highestContribution)
                .Select(x => x.Key);
            IEnumerable<uint> lowestContributors = contributions
                .Where(x => x.Value.Amount.Values.Sum() == lowestContribution)
                .Select(x => x.Key);*/


            if (success)
            {
                if (CurrentEmergency.SuccessTrackMod != null)
                    ServerTracks.Instance.ApplyModifier(CurrentEmergency.SuccessTrackMod);

                // everyone contributing to crisis gets infulence
                foreach (var kv in contributions)
                {
                    uint playerId = kv.Key;
                    int contributed = kv.Value;

                    if (contributed > 0)
                        ServerPlayers.Get(playerId).resources.ModifyInfluence(1);
                }

                // HighestBidderReward
                foreach (var id in highestContributors)
                ServerPlayers.Get(id).resources.ModifyMaterials(CurrentEmergency.HighestBidderReward);

                STC_CrisisResult result = new STC_CrisisResult(true, CurrentEmergency.SuccessReward, CurrentEmergency.SuccessTrackMod);
                NetworkManager.Server.SendToAll(result);
            }
            else 
            {
                if (CurrentEmergency.FailureTrackMod != null)
                    ServerTracks.Instance.ApplyModifier(CurrentEmergency.FailureTrackMod);

                // everyone looses materials
                if (CurrentEmergency.FailurePenalty != 0)
                {
                    foreach (var kv in contributions)
                    {
                        var player = ServerPlayers.Get(kv.Key);
                        if (player != null)
                            player.resources.ModifyMaterials(-CurrentEmergency.FailurePenalty);
                    }
                }

                // LowestBidderReward
                foreach (var id in lowestContributors)
                ServerPlayers.Get(id).resources.ModifyMaterials(CurrentEmergency.LowestBidderPenalty);

                STC_CrisisResult result = new STC_CrisisResult(false, 0, CurrentEmergency.FailureTrackMod);
                NetworkManager.Server.SendToAll(result);
            }
        }
    }
}