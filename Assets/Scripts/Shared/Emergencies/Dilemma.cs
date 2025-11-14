using UnityEngine;

namespace Client
{
    [CreateAssetMenu(fileName = "Dilemma", menuName = "Emergency/Dilemma")]
    public class Dilemma : Emergency
    {
        public override EmergencyType Type => EmergencyType.Dilemma;

        public string Keyword = "";

        [Header("Yes Consequence")]
        [TextArea(3, 10)] public string YesDescription;
        public TrackAmount YesTrackModifier;
        public int YesMaterialsModifier;
        public int YesInfluenceModifier;
        public string[] YesKeywordsToAdd;
        public string[] YesKeywordsToRemove;

        [Header("No Consequence")]
        [TextArea(3, 10)] public string NoDescription;
        public TrackAmount NoTrackModifier;
        public int NoMaterialsModifier;
        public int NoInfluenceModifier;
        public string[] NoKeywordsToAdd;
        public string[] NoKeywordsToRemove;
    }
}