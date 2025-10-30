using Client;
using TMPro;
using UnityEngine;

public class CrisisUI : Singleton<CrisisUI>
{
    [SerializeField] GameObject UI;

    [Header("Slide in/Out")]
    [SerializeField] Transform tablePosition;
    [SerializeField] Vector3 offScreenPosition;
    [SerializeField] float slideDuration = 1.2f;

    [Header("UI References")]
    [SerializeField] TextMeshProUGUI crisisNameText;
    [SerializeField] TextMeshProUGUI descriptionText;
    [SerializeField] TextMeshProUGUI infoText;
    [SerializeField] TextMeshProUGUI rewardsText;
    [SerializeField] SignatureDrawer signature;

    public void DisplayCrisis(Crisis crisis)
    {
        UI.SetActive(true);

        crisisNameText.text = crisis.CrisisName;
        descriptionText.text = crisis.Description;

        /*
        infoText.text =
            $"Resources: {string.Join(", ", crisis.AcceptedResourceTypes)}\n" +
            $"Success per Player: {crisis.SuccessPointsRequiredPerPlayer}\n" +
            $"Time Limit: {crisis.TimeToResolve}s\n" +
            $"Damage on Fail: {crisis.BunkerDamageOnFail}";

        //rewardsText.text =
            //BuildRewardText("Highest Bidder", crisis.HighestBidderReward) + "\n" +
            //BuildRewardText("Lowest Bidder", crisis.LowestBidderPenalty) + "\n" +
            //BuildRewardText("Failure", crisis.FailurePenalty);
        */
    }

    private string BuildRewardText(string title)
    {
        return title;
    }

    private void Update()
    {
        //if (signature.OnSignatureComplete())
        //SubmitContribution();
    }

    public void SubmitContribution()
    {
        // Player Contribution
        Debug.Log("Player submited contribution");
    }

    public void SlideIn()
    {

    }

    public void SlideOut()
    {

    }
}