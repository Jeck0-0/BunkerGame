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
        
        public float economyDuration;
        public float crisisDuration;

        protected override void Awake()
        {
            base.Awake();
            if (!crisisPhase) crisisPhase = GetComponent<CrisisPhase>();
            if (!dilemmaPhase) dilemmaPhase = GetComponent<DilemmaPhase>();
        }
        
        [Button]
        public void GameStart()
        {
            StartCoroutine(GameLoop());
        }
        
        protected IEnumerator GameLoop()
        {
            // debug crisis
            while (true)
            {
                yield return crisisPhase.PlayPhase();
                yield return Helpers.GetWait(crisisDuration);
            }
            
            while (true)
            {
                int economyPhases = UnityEngine.Random.Range(3, 5);
                for (int i = 0; i < economyPhases; i++)
                {                
                    yield return dilemmaPhase.PlayPhase();
                    yield return Helpers.GetWait(economyDuration);
                }

                yield return crisisPhase.PlayPhase();
                yield return Helpers.GetWait(crisisDuration);
            }
        }

        

    }
}