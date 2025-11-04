using UnityEngine;

namespace Client
{
    [CreateAssetMenu(fileName = "Dilemma", menuName = "Emergency/Dilemma")]
    public class Dilemma : Emergency
    {
        public override EmergencyType Type => EmergencyType.Dilemma;
        
        [TextArea(3, 10)] public string YesConsequence;
        [TextArea(3, 10)] public string NoConsequence;

        public TrackAmount YesTrackModifier;
        public TrackAmount NoTrackModifier;
    }
}