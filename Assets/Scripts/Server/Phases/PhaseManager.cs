using System;
using System.Collections;
using Networking;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Server
{
    public class PhaseManager : Singleton<PhaseManager>
    {
        public CrisisPhase crisisPhase;
        public DilemmaPhase dilemmaPhase;

        public int totalCrises = 5;

        public float economyDuration;
        public float emergencyDuration;

        protected int crisesCompleted = 0;

        protected override void Awake()
        {
            base.Awake();
            if (!crisisPhase) crisisPhase = GetComponent<CrisisPhase>();
            if (!dilemmaPhase) dilemmaPhase = GetComponent<DilemmaPhase>();
            ServerTracks.Instance.ResourceReachedZero += OnTrackReachedZero;
        }

        [Button]
        public void GameStart()
        {
            StartCoroutine(GameLoop());
            Debug.Log("Game started");
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

            while (crisesCompleted < totalCrises)
            {
                // Economy phase
                Debug.Log("Economy phase started");
                EconomyPhase();
                yield return Helpers.GetWait(economyDuration);

                // emergency phase
                // Randomly chose dilemma or crisis phases
                Debug.Log("Dilemma phase started");
                yield return dilemmaPhase.PlayPhase();

                yield return Helpers.GetWait(1f);

                Debug.Log("Crisis phase started");
                yield return crisisPhase.PlayPhase();

                crisesCompleted++;
                yield return Helpers.GetWait(emergencyDuration);
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
            }

            // Let clients know about resource changes
        }

        void EndGame()
        {
            Debug.Log("Game ended");
        }
    }
}