using UnityEngine;

namespace Client
{
    [CreateAssetMenu(fileName = "Crisis", menuName = "Emergency/Crisis")]
    public class Crisis : Emergency
    {
        public override EmergencyType Type => EmergencyType.Crisis;

        public string Keyword = "";

        [Tooltip("Amount of materials needed for resolving this crisis")]
        public int requiredMaterials;


        [Tooltip("Damage applied to bunker on failure")]
        public int BunkerDamageOnFail = 1;
       
        
        //to rebalance
        [Header("Success")] 
        public int HighestBidderInfluenceReward;
        public int HighestBidderVPReward;
        public int SuccessInfluenceReward;
        public TrackAmount SuccessTrackMod;
        public string[] SuccessKeywordsToAdd;

        [Header("Failure")]
        public int LowestBidderPenalty;
        public int FailurePenalty;
        public TrackAmount FailureTrackMod;
        public string[] FailureKeywordsToAdd;
    }
}