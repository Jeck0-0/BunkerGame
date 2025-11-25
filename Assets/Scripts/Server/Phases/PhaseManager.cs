using System;
using Networking;
using Packets;
using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Server
{
    public class PhaseManager : Singleton<PhaseManager>
    {
        public CrisisPhase crisisPhase;
        public DilemmaPhase dilemmaPhase;
        public bool startOnAwake = true;
        
        [Header("Game Settings")]
        public int totalCrises = 5;
        public int minTurnsBetweenCrises = 1;
        public float baseCrisisChance = 0.2f;
        public float crisisChanceGrowth = 0.15f;
        public float maxCrisisChance = 1f;

        [Header("Phase Durations")]
        public float economyDuration;
        public float emergencyDelay;

        protected int crisesCompleted = 0;
        private int turnsSinceLastCrisis = 0;
        private float currentCrisisChance;

        protected override void Awake()
        {
            base.Awake();
            if (!crisisPhase) crisisPhase = GetComponent<CrisisPhase>();
            if (!dilemmaPhase) dilemmaPhase = GetComponent<DilemmaPhase>();
            ServerTracks.Instance.ResourceReachedZero += OnTrackReachedZero;
        }

        private void Start()
        {
            if (startOnAwake)
                SecretObjectiveManager.Instance.SetSecretObjectives();
        }

        [Button]
        public void GameStart()
        {
            Debug.Log("Game started");
            NetworkManager.Server.SendToAll(new STC_GameStart());
            StartCoroutine(GameLoop());
        }

        private void OnDestroy()
        {
            if (ServerTracks.Instance != null)
            ServerTracks.Instance.ResourceReachedZero -= OnTrackReachedZero;
        }

        private void OnTrackReachedZero(TrackType t)
        {
            Debug.Log($"[PhaseManager] Track {t} reached zero");
            StopAllCoroutines();
            EndGame(); // everyone loses
        }

        protected IEnumerator GameLoop()
        {
            crisesCompleted = 0;
            turnsSinceLastCrisis = 0;
            currentCrisisChance = baseCrisisChance;

            while (crisesCompleted < totalCrises)
            {
                // Economy phase
                Debug.Log("Economy phase started");
                EconomyPhase();
                yield return Helpers.GetWait(economyDuration);

                // emergency phase
                yield return PlayRandomEmergencyPhase();

                yield return Helpers.GetWait(emergencyDelay);
            }

            // If no crisis left end game
            EndGame();
        }

        void EconomyPhase()
        {
            foreach (var p in ServerPlayers.GetAll())
            {
                p.resources.ModifyMaterials(1);
                p.resources.ModifyInfluence(2);

                // update clients about resource changes
                NetworkManager.Server.SendTo(p.id, new STC_UpdateResources(p.resources.Materials, p.resources.Influence));
            }
        }
        IEnumerator PlayRandomEmergencyPhase()
        {
            bool canTriggerCrisis = turnsSinceLastCrisis >= minTurnsBetweenCrises;
            bool chooseCrisis = false;

            if (canTriggerCrisis)
            {
                float R = Random.value;
                chooseCrisis = R < currentCrisisChance;
            }

            if (chooseCrisis)
            {
                Debug.Log($"Crisis Phase (chance: {currentCrisisChance})");
                yield return crisisPhase.PlayPhase();

                crisesCompleted++;
                turnsSinceLastCrisis = 0;
                currentCrisisChance = baseCrisisChance;
            }
            else
            {
                Debug.Log($"Dilemma Phase (chance: {currentCrisisChance})");
                yield return dilemmaPhase.PlayPhase();

                turnsSinceLastCrisis++;
                if (canTriggerCrisis)
                    currentCrisisChance = Mathf.Min(currentCrisisChance + crisisChanceGrowth, maxCrisisChance);
            }
        }

        void EndGame()
        {
            Debug.Log("Game ended");
        }
    }
}