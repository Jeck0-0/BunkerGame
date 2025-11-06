using Client;
using Networking;
using Packets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Server
{
    public class DilemmaPhase : MonoBehaviour
    {
        protected Dilemma[] dilemmaPool;
        protected Dilemma CurrentDilemma;

        protected struct Vote
        {
            public int OptionIndex;
            public int InfluenceSpent;
            public bool Withheld;
        }

        protected Dictionary<uint, Vote> votes = new();
        protected Dictionary<int, int> sideTotal = new();

        private void Awake()
        {
            dilemmaPool = Resources.LoadAll<Dilemma>("ScriptableObjects/Emergencies/Dilemma");
        }

        public IEnumerator PlayPhase()
        {
            StartRandomDilemma();

            // Wait for all votes or timer end
            NetworkManager.Server.Subscribe<CTS_VoteOnDilemma>(ReceiveVote);
            float endTime = Time.time + CurrentDilemma.TimeToResolve;
            yield return new WaitUntil(() => AllPlayersVoted() || Time.time > endTime);
            NetworkManager.Server.Unsubscribe<CTS_VoteOnDilemma>(ReceiveVote);

            CalculateDilemmaResult();
        }

        protected void StartRandomDilemma()
        {
            // Get random dilemma
            if (dilemmaPool == null || dilemmaPool.Length == 0) Debug.LogError("No dilemmas in pool");
            CurrentDilemma = GetRandomAccessibleDilemma();

            if (CurrentDilemma == null)
            {
                Debug.LogError("No accessible dilemmas found");
                return;
            }


            // Send Dilemma start packet
            STC_StartEmergency packet = new STC_StartEmergency(CurrentDilemma, DateTime.Now.Ticks);
            NetworkManager.Server.SendToAll(packet);

            // Prepare to receive votes
            votes.Clear();
            sideTotal.Clear();
        }

        protected Dilemma GetRandomAccessibleDilemma()
        {
            var accessible = new List<Dilemma>();

            foreach (var dilemma in dilemmaPool)
            {
                if (string.IsNullOrEmpty(dilemma.Keyword) || KeywordManager.Instance.Has(dilemma.Keyword))
                accessible.Add(dilemma);
            }

            if (accessible.Count == 0)
            {
                Debug.LogWarning("No accessible dilemmas found");
                return null;
            }

            return accessible[UnityEngine.Random.Range(0, accessible.Count)];
        }

        protected void ReceiveVote(uint player, BasePacket packet)
        {
            var votePacket = packet as CTS_VoteOnDilemma;
            if (votePacket == null) return;

            if (votes.ContainsKey(player))
            {
                Debug.LogWarning($"Player {player} sent multiple votes (not allowed smh)");
                return;
            }

            // OptionIndex, -1 means withold
            bool withheld = votePacket.OptionIndex == -1;


            int available = 10;   // Replace with actual player influence later
            //![player].Tracks.Has(contributionPacket.TrackAmount)) DOESNT HAVE RESOURCES
            if (votePacket.InfluenceSpent < 0)
            {
                Debug.LogWarning($"Player {player} sent negative influence.");
                return;
            }
            if (!withheld && votePacket.InfluenceSpent > available)
            {
                Debug.LogWarning($"Player {player} tried to spend more influence ({votePacket.InfluenceSpent}) than available ({available}).");
                // clamp to available
                votePacket.InfluenceSpent = available;
            }

            votes[player] = new Vote
            {
                OptionIndex = votePacket.OptionIndex,
                InfluenceSpent = votePacket.InfluenceSpent,
                Withheld = withheld
            };

            // Add to side total
            if (!withheld)
            {
                if (!sideTotal.ContainsKey(votePacket.OptionIndex))
                sideTotal[votePacket.OptionIndex] = 0;

                sideTotal[votePacket.OptionIndex] += votePacket.InfluenceSpent;
            }
        }


        protected void CalculateDilemmaResult()
        {
            if (sideTotal.Count == 0)
            {
                // add tie breaker
                Debug.LogWarning("No votes received");
                sideTotal[1] = 0;
            }

            int bestValue = -1;
            List<int> bestOptions = new();

            for (int i = 0; i < 2; i++)
            {
                if (!sideTotal.ContainsKey(i)) sideTotal[i] = 0;

                int value = sideTotal[i];
                if (value > bestValue)
                {
                    bestValue = value;
                    bestOptions.Clear();
                    bestOptions.Add(i);
                }
                else if (value == bestValue)
                {
                    bestOptions.Add(i);
                }
            }

            int winningOption = bestOptions[0];
            if (bestOptions.Count > 1) winningOption = 0; // add tie-breaker

            // Apply modifiers
            TrackAmount modifier = CurrentDilemma.NoTrackModifier;
            if (winningOption == 0) modifier = CurrentDilemma.YesTrackModifier;

            ServerTracks.Instance?.ApplyModifier(modifier);

            ApplyKeywordChanges(CurrentDilemma, winningOption == 0);

            // Send results
            STC_DilemmaResult result = new STC_DilemmaResult(winningOption, modifier);
            NetworkManager.Server.SendToAll(result);
        }

        private void ApplyKeywordChanges(Dilemma dilemma, bool yesWon)
        {
            if (yesWon)
            {
                KeywordManager.Instance.AddMultiple(dilemma.YesKeywordsToAdd);
                KeywordManager.Instance.RemoveMultiple(dilemma.YesKeywordsToRemove);
            }
            else
            {
                KeywordManager.Instance.AddMultiple(dilemma.NoKeywordsToAdd);
                KeywordManager.Instance.RemoveMultiple(dilemma.NoKeywordsToRemove);
            }
        }

        protected bool AllPlayersVoted()
        {
            /*
            var all = ServerPlayers.Instance;
            if (all == null) return false;

            return votes.Count >= all.players.count; // simple version
            */
            return false;
        }
    }
}