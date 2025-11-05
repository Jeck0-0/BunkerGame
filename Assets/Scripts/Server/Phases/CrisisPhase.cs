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

        protected Dictionary<uint, TrackAmount> contributions = new ();
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
            yield return new WaitUntil(() => contributions.Count > 1 || Time.time > endTime);
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
            if (false)  //![player].Tracks.Has(contributionPacket.TrackAmount)) DOESNT HAVE RESOURCES
            {
                Debug.LogWarning($"Player {player} tried to contribute more resources than they have {contributionPacket.TrackAmount}");
                return;
            }
            
            contributions[player] = contributionPacket.TrackAmount;
            
            // Check if the player contributed unnecessary resources (shouldn't be able to)
            if (contributionPacket.TrackAmount.Values.Keys.All(x => CurrentEmergency.requiredTracks.Values.ContainsKey(x)))
                Debug.LogWarning("Player contributed unnecessary resources: " + player);
            
            // Remove resources from player inv
            //GameManager.Players[player].Tracks -= contributionPacket.TrackAmount; REMOVE RESOURCES
        }

        protected void CalculateCrisisResult()
        {
            //crisis result
            TrackAmount totalContributions = new TrackAmount();
            foreach (var contribution in contributions.Values)
                totalContributions += contribution;

            bool success = totalContributions.Has(CurrentEmergency.requiredTracks);

            // highest and lowest bidders
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
                 STC_CrisisResult result = new STC_CrisisResult(true, CurrentEmergency.SuccessReward);
                 NetworkManager.Server.SendToAll(result);
            }
            else 
            {
                STC_CrisisResult result = new STC_CrisisResult(false, null);
                NetworkManager.Server.SendToAll(result);
            }
        }
    }
}