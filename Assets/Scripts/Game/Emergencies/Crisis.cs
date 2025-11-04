using UnityEngine;

namespace Client
{
    [CreateAssetMenu(fileName = "Crisis", menuName = "Emergency/Crisis")]
    public class Crisis : Emergency
    {
        public override EmergencyType Type => EmergencyType.Crisis;
        
        [Tooltip("What resources are used for resolving this crisis")]
        public TrackAmount requiredTracks;


        [Tooltip("Damage applied to bunker on failure")]
        public int BunkerDamageOnFail = 1;
        
        [Header("Rewards/Penalties")] 
        public TrackAmount HighestBidderReward;
        public TrackAmount SuccessReward;
        public TrackAmount LowestBidderReward;
        
        public TrackAmount FailurePenalty;
    }
}