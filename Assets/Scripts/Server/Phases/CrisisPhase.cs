using System;
using System.Collections;
using System.Collections.Generic;
using Client;
using Networking;
using Packets;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Server
{
    public class CrisisPhase : MonoBehaviour
    {
        public List<Crisis> crisisPool = new ();

        private Dictionary<uint, ResourceAmount> contributions = new ();
        
        public IEnumerator PlayPhase()
        {
            // Get random crisis
            if (crisisPool == null || crisisPool.Count == 0) Debug.LogError("No crisis in pool");
            var crisis = crisisPool[Random.Range(0, crisisPool.Count)];
            
            // Send crisis start packet
            STC_StartCrisisPhase packet = new STC_StartCrisisPhase(DateTime.Now.Ticks, crisis.TimeToResolve);
            NetworkManager.Server.SendToAll(packet);
            
            // Prepare to receive contributions
            contributions.Clear();
            NetworkManager.Server.Subscribe<CTS_ContributeToCrisis>(ReceiveContribution);

            // Wait for all contribution or timer end
            float endTime = Time.time + crisis.TimeToResolve;
            yield return new WaitUntil(() => contributions.Count > 1 || Time.time > endTime);
            
            NetworkManager.Server.Unsubscribe<CTS_ContributeToCrisis>(ReceiveContribution);
            
            //crisis result
            
            yield return null;
        }

        protected void ReceiveContribution(uint player, BasePacket packet)
        {
            CTS_ContributeToCrisis info = packet as CTS_ContributeToCrisis;
            if (contributions.ContainsKey(player))
            {
                Debug.LogWarning($"Player {player} sent multiple contributions (not allowed smh)");
                return;
            }
            if (!GameManager.Players[player].resources.Has(info.resourceAmount))
            {
                Debug.LogWarning($"Player {player} tried to contribute more resources than they have {info.resourceAmount}");
                return;
            }
            
            // Store contribution
            contributions[player] = info.resourceAmount;
            
            // Remove resources from player inv
            GameManager.Players[player].resources -= info.resourceAmount;
        }
    }
}