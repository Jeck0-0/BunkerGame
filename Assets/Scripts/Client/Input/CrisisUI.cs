using System.Collections;
using Client;
using Networking;
using Packets;
using TMPro;
using UnityEngine;

public class CrisisUI : Singleton<CrisisUI>
{
    [SerializeField] GameObject UI;

    [Header("Slide in/Out")]
    [SerializeField] Transform tablePosition;
    [SerializeField] Vector3 offScreenPosition;
    [SerializeField] float slideDuration = 1.2f;
    [SerializeField] AnimationCurve slideCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("UI References")]
    [SerializeField] TextMeshProUGUI crisisNameText;
    [SerializeField] TextMeshProUGUI descriptionText;
    [SerializeField] TextMeshProUGUI infoText;
    [SerializeField] TMP_InputField contributionField;
    [SerializeField] SignatureDrawer signature;

    private Crisis currentCrisis;
    private Coroutine slide;

    void Start()
    {
        contributionField.contentType = TMP_InputField.ContentType.IntegerNumber;
        contributionField.ForceLabelUpdate();

        UI.transform.position = offScreenPosition;
        UI.SetActive(false);
    }

    public void DisplayCrisis(Crisis crisis)
    {
        currentCrisis = crisis;
        UI.SetActive(true);

        crisisNameText.text = crisis.Title;
        descriptionText.text = crisis.Description;
        infoText.text =  $"Required Materials: {crisis.requiredMaterials}\n" + $"Bunker Damage on Fail: {crisis.BunkerDamageOnFail}";

        SlideIn();
    }
    private void Update()
    {
        if (signature.OnSignatureComplete())
            SubmitContribution();
    }

    public void SubmitContribution()
    {
        if (!int.TryParse(contributionField.text, out int amount)) return;
        if (amount <= 0) return;

        if (ClientResources.Instance.Materials < amount)
        {
            Debug.Log("Not enough materials");
            return;
        }

        Debug.Log("Player submited contribution");

        //reduce immediately from client view
        ClientResources.Instance.ModifyMaterials(-amount);

        // Send packet to server
        NetworkManager.Client.Send(new CTS_ContributeToCrisis(amount));

        contributionField.interactable = false;
        signature.ClearSignature();
    }

    public void DisplayCrisisResult(bool success, TrackAmount trackMod)
    {
        string resultText = success ? "<color=green>SUCCESS</color>" : "<color=red>FAILURE</color>";
        infoText.text = $"Crisis Result: {resultText}";

        ClientTracks.Instance.ApplyModifier(trackMod);

        contributionField.interactable = true;

        SlideOut();
    }

    public void SlideIn()
    {
        if (slide != null)
        StopCoroutine(slide);
        slide = StartCoroutine(Slide(offScreenPosition, tablePosition.position, true));
    }


    public void SlideOut()
    {
        if (slide != null)
        StopCoroutine(slide);
        slide = StartCoroutine(Slide(tablePosition.position, offScreenPosition, false));
    }

    private IEnumerator Slide(Vector3 from, Vector3 to, bool visible)
    {
        float t = 0f;
        while (t < slideDuration)
        {
            t += Time.deltaTime;
            float normalized = t / slideDuration;
            float curveValue = slideCurve.Evaluate(normalized);
            UI.transform.position = Vector3.LerpUnclamped(from, to, curveValue);
            yield return null;
        }

        UI.transform.position = to;
        UI.SetActive(visible);
    }
}