using System.Collections.Generic;
using UnityEngine;

namespace Client
{
    [CreateAssetMenu(fileName = "Crisis", menuName = "Scriptable Objects/Crisis")]
    public class Crisis : ScriptableObject
    {
        public string CrisisId;
        public string CrisisName;

        [TextArea(3, 10)] public string Description;

        [Tooltip("What resources are used for resolving this crisis")]
        public ResourceAmount RequiredResources;

        [Tooltip("How much time players have to resolve the crisis(in seconds)")]
        public float TimeToResolve = 120f;

        [Tooltip("Damage applied to bunker on failure")]
        public int BunkerDamageOnFail = 1;

        [Tooltip("If true players commits are hidden")]
        public bool IsBetHidden = true;

        [Header("Rewards/Penalties")] public ResourceAmount HighestBidderReward;
        public ResourceAmount LowestBidderPenalty;
        public ResourceAmount FailurePenalty;
    }
}