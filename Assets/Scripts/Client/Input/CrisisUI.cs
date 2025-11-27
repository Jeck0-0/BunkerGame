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

    [Header("Look Around")]
    [SerializeField] Vector3 lookAroundOffset = new Vector3(0, -4f, 0);
    [SerializeField] Vector3 lookAroundRotation = new Vector3(50f, 0f, 0f);
    [SerializeField] Vector3 pauseOffset = new Vector3(0, -1.8f, -0.8f);
    [SerializeField] Vector3 pauseRotation = new Vector3(10f, 0f, 0f);
    [SerializeField] float lookAroundSpeed = 5f;

    private bool documentIsOut = false;
    private bool resultBeingShown = false;
    private float resultTime;
    private bool contributingLocked;
    private GameObject activeUI;
    private bool isPeeking;

    private Coroutine crisisSlide;
    private Coroutine resultSlide;

    void Start()
    {
        contributionField.contentType = TMP_InputField.ContentType.Custom;
        contributionField.onValidateInput += OnlyDigits;

        crisisUI.SetActive(false);
        resultUI.SetActive(false);
        contributingLocked = false;
    }

    private void Update()
    {
        if (documentIsOut) DocumentDown();
        if (ComputerManager.Instance.GetComputerUp()) return;

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

    private void DocumentDown()
    {
        if (activeUI != null)
        {
            Vector3 targetPos;
            Quaternion targetRot;

            if (ComputerManager.Instance.GetComputerUp())
            {
                isPeeking = true;
                targetPos = tablePosition.position + pauseOffset;
                targetRot = Quaternion.Euler(pauseRotation);
            }
            else
            {
                isPeeking = Input.GetMouseButton(1);
                targetPos = isPeeking ? tablePosition.position + lookAroundOffset : tablePosition.position;
                targetRot = isPeeking ? Quaternion.Euler(lookAroundRotation) : Quaternion.identity;
            }

            activeUI.transform.position = Vector3.Lerp(activeUI.transform.position, targetPos, Time.deltaTime * lookAroundSpeed);
            activeUI.transform.rotation = Quaternion.Lerp(activeUI.transform.rotation, targetRot, Time.deltaTime * lookAroundSpeed);
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
            signature.ClearSignature();
            return;
        }

        Debug.Log("Player submited contribution");
        contributingLocked = true;

        //reduce immediately from client view
        ClientResources.Instance.ModifyMaterials(-amount);

        // Send packet to server
        SteamClient.Send(new CTS_ContributeToCrisis(amount));

        contributionField.interactable = false;

        SlideOut(crisisUI);
    }

    public void DisplayCrisisResult(bool success, TrackAmount trackMod)
    {
        string result = success ? "<color=green>SUCCESS</color>" : "<color=red>FAILURE</color>";
        resultText.text = $"Crisis Result: {result}";

        ClientTracks.Instance.ApplyModifier(trackMod);
        resultBeingShown = true;
        SlideIn(resultUI);
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
        activeUI = ui;
        documentIsOut = false;
        ui.transform.rotation = Quaternion.identity;

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
        if (activeUI == ui) activeUI = null;
        documentIsOut = false;

        if (ui == resultUI)
        {
            if (resultSlide != null) StopCoroutine(resultSlide);
            resultSlide = StartCoroutine(Slide(ui, false));
            resultBeingShown = false;
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

        Vector3 start = visible ? offScreenPosition : ui.transform.position;
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
        if (visible) documentIsOut = true;
    }

    private char OnlyDigits(string text, int charIndex, char addedChar)
    {
        // Only allow digits
        return char.IsDigit(addedChar) ? addedChar : '\0';
    }
}