using System.Collections;
using Client;
using Networking;
using Packets;
using TMPro;
using UnityEngine;

public class CrisisUI : Singleton<CrisisUI>
{
    [Header("Crisis UI")]
    [SerializeField] GameObject crisisUI;
    [SerializeField] TextMeshProUGUI crisisHeader;
    [SerializeField] TextMeshProUGUI descriptionText;
    [SerializeField] TextMeshProUGUI infoText;
    [SerializeField] TMP_InputField contributionField;
    [SerializeField] SignatureDrawer signature;

    [Header("Result UI")]
    [SerializeField] GameObject resultUI;
    [SerializeField] TextMeshProUGUI resultHeader;
    [SerializeField] TextMeshProUGUI resultText;
    [SerializeField] float delayBeforeRemovingResult = 1f;

    [Header("Slide in/Out")]
    [SerializeField] Transform tablePosition;
    [SerializeField] Vector3 offScreenPosition;
    [SerializeField] float slideDuration = 1.2f;
    [SerializeField] AnimationCurve slideCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private bool resultBeingShown = false;
    private float resultTime;
    private bool contributingLocked;

    private Coroutine crisisSlide;
    private Coroutine resultSlide;

    void Start()
    {
        contributionField.contentType = TMP_InputField.ContentType.IntegerNumber;
        contributionField.ForceLabelUpdate();

        crisisUI.SetActive(false);
        resultUI.SetActive(false);
        contributingLocked = false;
    }

    private void Update()
    {
        if (signature.OnSignatureComplete())
            SubmitContribution();

        if (!resultBeingShown) return;

        resultTime += Time.deltaTime;

        if (Input.GetMouseButtonDown(0) && resultTime >= delayBeforeRemovingResult)
        {
            SlideOut(resultUI);
            resultTime = 0f;
            resultBeingShown = false;
        }
    }

    public void DisplayCrisis(Crisis crisis)
    {
        ResetUI();
        crisisUI.SetActive(true);

        crisisHeader.text = crisis.Title;
        resultHeader.text = crisis.Title;
        descriptionText.text = crisis.Description;
        infoText.text =  $"Required Materials: {crisis.requiredMaterials}\n" + $"Bunker Damage on Fail: {crisis.BunkerDamageOnFail}";

        SlideIn(crisisUI);
    }

    public void SubmitContribution()
    {
        if (contributingLocked) return;

        if (!int.TryParse(contributionField.text, out int amount)) return;
        if (amount <= 0) return;

        if (ClientResources.Instance.Materials < amount)
        {
            Debug.Log("Not enough materials");
            return;
        }

        Debug.Log("Player submited contribution");
        contributingLocked = true;

        //reduce immediately from client view
        ClientResources.Instance.ModifyMaterials(-amount);

        // Send packet to server
        NetworkManager.Client.Send(new CTS_ContributeToCrisis(amount));

        contributionField.interactable = false;

        SlideOut(crisisUI);
        SlideIn(resultUI);
    }

    public void DisplayCrisisResult(bool success, TrackAmount trackMod)
    {
        string resultText = success ? "<color=green>SUCCESS</color>" : "<color=red>FAILURE</color>";
        infoText.text = $"Crisis Result: {resultText}";

        ClientTracks.Instance.ApplyModifier(trackMod);
        resultBeingShown = true;
    }
    private void ResetUI()
    {
        resultTime = 0f;
        contributingLocked = false;
        contributionField.interactable = true;
        contributionField.text = string.Empty;
        signature.ClearSignature();

        crisisUI.SetActive(false);
        resultUI.SetActive(false);
    }

    private void SlideIn(GameObject ui)
    {
        if (ui == resultUI)
        {
            if (resultSlide != null) StopCoroutine(resultSlide);
            resultSlide = StartCoroutine(Slide(ui, true));
        }
        else
        {
            if (crisisSlide != null) StopCoroutine(crisisSlide);
            crisisSlide = StartCoroutine(Slide(ui, true));
        }
    }

    private void SlideOut(GameObject ui)
    {
        if (ui == resultUI)
        {
            if (resultSlide != null) StopCoroutine(resultSlide);
            resultSlide = StartCoroutine(Slide(ui, false));
        }
        else
        {
            if (crisisSlide != null) StopCoroutine(crisisSlide);
            crisisSlide = StartCoroutine(Slide(ui, false));
        }
    }

    private IEnumerator Slide(GameObject ui, bool visible)
    {
        if (visible) ui.SetActive(true);

        Vector3 start = visible ? offScreenPosition : tablePosition.position;
        Vector3 end = visible ? tablePosition.position : offScreenPosition;

        float t = 0f;
        while (t < slideDuration)
        {
            t += Time.deltaTime;
            float curve = slideCurve.Evaluate(t / slideDuration);
            ui.transform.position = Vector3.LerpUnclamped(start, end, curve);
            yield return null;
        }

        ui.transform.position = end;
        if (!visible) ui.SetActive(false);
    }
}