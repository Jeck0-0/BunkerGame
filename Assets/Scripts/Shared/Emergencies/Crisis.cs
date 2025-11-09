using UnityEngine;

namespace Client
{
    [CreateAssetMenu(fileName = "Crisis", menuName = "Emergency/Crisis")]
    public class Crisis : Emergency
    {
        public override EmergencyType Type => EmergencyType.Crisis;
        
        [Tooltip("Amount of materials needed for resolving this crisis")]
        public int requiredMaterials;


        [Tooltip("Damage applied to bunker on failure")]
        public int BunkerDamageOnFail = 1;
       
        
        //to rebalance
        [Header("Rewards/Penalties")] 
        public int HighestBidderReward;
        public int SuccessReward;
        public TrackAmount SuccessTrackMod;
        public int LowestBidderPenalty;
        
        public int FailurePenalty;
        public TrackAmount FailureTrackMod;
    }
}