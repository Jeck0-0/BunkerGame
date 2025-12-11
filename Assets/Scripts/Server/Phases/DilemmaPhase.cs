using Client;
using Networking;
using Packets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Server
{
    public class DilemmaPhase : MonoBehaviour
    {
        private List<Dilemma> dilemmaPool;
        public float dilemmaEndDelay = 2f;
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
            var loadedDilemmas = Resources.LoadAll<Dilemma>("ScriptableObjects/Emergencies/Dilemma");
            dilemmaPool = new List<Dilemma>(loadedDilemmas);
        }

        public IEnumerator PlayPhase()
        {
            StartRandomDilemma();

            if (CurrentDilemma == null)
            {
                Debug.LogError("No accessible dilemma found");
                yield break;
            }

            // Wait for all votes or timer end
            GameServer.Subscribe<CTS_VoteOnDilemma>(ReceiveVote);
            float endTime = Time.time + CurrentDilemma.TimeToResolve;
            yield return new WaitUntil(() => votes.Count >= ServerPlayers.GetAll().Count() || Time.time > endTime);
            GameServer.Unsubscribe<CTS_VoteOnDilemma>(ReceiveVote);

            CalculateDilemmaResult();
            yield return new WaitForSeconds(dilemmaEndDelay);
        }

        protected void StartRandomDilemma()
        {
            // Get random dilemma
            if (dilemmaPool == null || dilemmaPool.Count == 0) Debug.LogError("No dilemmas in pool");
            CurrentDilemma = GetRandomAccessibleDilemma();

            if (CurrentDilemma == null)
            {
                Debug.LogError("No accessible dilemmas found");
                return;
            }

            if (!CurrentDilemma.Repeatable)
                dilemmaPool.Remove(CurrentDilemma);

            // Send Dilemma start packet
            STC_StartEmergency packet = new STC_StartEmergency(CurrentDilemma, DateTime.Now.Ticks);
            GameServer.SendToAll(packet);

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

            // Option, -1 means withold
            bool withheld = votePacket.OptionIndex == -1;

            int available = ServerPlayers.Get(player).resources.Influence;

            if (votePacket.InfluenceSpent < 0)
            {
                Debug.LogWarning($"Player {player} sent negative influence.");
                return;
            }
            if (!withheld && votePacket.InfluenceSpent > available)
            {
                Debug.LogWarning($"Player {player} tried to contribute more materials than they have {votePacket.InfluenceSpent}");
                // clamp to available
                votePacket.InfluenceSpent = available;
            }

            votes[player] = new Vote
            {
                OptionIndex = votePacket.OptionIndex,
                InfluenceSpent = votePacket.InfluenceSpent,
                Withheld = withheld
            };

            GameServer.SendToAll(new STC_PlayerVoted(player, votePacket.OptionIndex, votePacket.InfluenceSpent));

            // Add to side total
            if (!withheld)
            {
                if (!sideTotal.ContainsKey(votePacket.OptionIndex))
                sideTotal[votePacket.OptionIndex] = 0;

                sideTotal[votePacket.OptionIndex] += votePacket.InfluenceSpent;

                ServerPlayers.Get(player).resources.ModifyInfluence(-votePacket.InfluenceSpent);
            }
        }

        protected void CalculateDilemmaResult()
        {
            if (sideTotal.Count == 0)
            {
                Debug.LogWarning("No votes received");
                sideTotal[1] = 0;
            }

            int winningOption = WinningOption();
            ApplyDilemmaEffects(winningOption);

            var trackModifier = winningOption == 0 ? CurrentDilemma.YesTrackModifier : CurrentDilemma.NoTrackModifier;
            GameServer.SendToAll(new STC_DilemmaResult(winningOption, trackModifier));
        }

        private int WinningOption()
        {
            int bestValue = -1;
            List<int> bestOptions = new(2);

            for (int i = 0; i < 2; i++)
            {
                int value = sideTotal.TryGetValue(i, out int v) ? v : 0;

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

            return bestOptions.Count > 1 ? TieBreaker(bestOptions) : bestOptions[0];
        }

        private void ApplyDilemmaEffects(int winningOption)
        {
            bool yesWon = winningOption == 0;

            TrackAmount track = yesWon ? CurrentDilemma.YesTrackModifier : CurrentDilemma.NoTrackModifier;

            ServerTracks.Instance.ApplyModifier(track);

            ApplyResourceChanges(yesWon);
            ApplyKeywordChanges(CurrentDilemma, yesWon);
        }

        private void ApplyResourceChanges(bool yesWon)
        {
            var players = ServerPlayers.GetAll();
            int mat = yesWon ? CurrentDilemma.YesMaterialsModifier : CurrentDilemma.NoMaterialsModifier;
            int inf = yesWon ? CurrentDilemma.YesInfluenceModifier : CurrentDilemma.NoInfluenceModifier;

            if (mat == 0 && inf == 0) return;

            foreach (var player in players)
            {
                player.resources.ModifyMaterials(mat);
                player.resources.ModifyInfluence(inf);
            }
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
        protected virtual int TieBreaker(List<int> tiedOptions)
        {
            int result = UnityEngine.Random.Range(0, tiedOptions.Count);
            int chosen = tiedOptions[result];
            Debug.Log($"[DilemmaPhase] Tie between options {string.Join(",", tiedOptions)}, {chosen} is chosen");
            return chosen;
        }
    }
}