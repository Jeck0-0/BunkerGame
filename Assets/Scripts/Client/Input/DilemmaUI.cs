using Client;
using Networking;
using Packets;
using System.Collections;
using TMPro;
using UnityEngine;

public class DilemmaUI : Singleton<DilemmaUI>
{
    [Header("Voting UI")]
    [SerializeField] GameObject votingUI;
    [SerializeField] TextMeshProUGUI votingHeader;
    [SerializeField] TextMeshProUGUI descriptionText;
    [SerializeField] TMP_InputField influenceField;
    [SerializeField] TextMeshProUGUI yesText;
    [SerializeField] SignatureDrawer yesCheckBox;
    [SerializeField] TextMeshProUGUI noText;
    [SerializeField] SignatureDrawer noCheckBox;
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
    [SerializeField] float lookAroundSpeed = 5f;

    private bool documentIsOut = false;
    private bool resultBeingShown = false;
    private bool votingLocked;
    private float resultTime;
    private GameObject activeUI;
    private bool isPeeking;

    private Coroutine votingSlide;
    private Coroutine resultSlide;
    private Dilemma currentDilema;

    void Start()
    {
        influenceField.contentType = TMP_InputField.ContentType.Custom;
        influenceField.onValidateInput += OnlyDigits;
        votingUI.SetActive(false);
        resultUI.SetActive(false);
        documentIsOut = false;

        yesCheckBox.OnSigned += () => ExclusiveSignature(yesCheckBox, noCheckBox);
        noCheckBox.OnSigned += () => ExclusiveSignature(noCheckBox, yesCheckBox);
    }
    private void Update()
    {
        if (documentIsOut) DocumentDown();

        if (signature.OnSignatureComplete())
        {
            if (yesCheckBox.OnSignatureComplete()) SubmitVote(0);
            else if (noCheckBox.OnSignatureComplete()) SubmitVote(1);
        }

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
            if (Input.GetMouseButton(1)) isPeeking = true;
            else isPeeking = false;

            Vector3 targetPos = isPeeking ? tablePosition.position + lookAroundOffset : tablePosition.position;
            Quaternion targetRot = isPeeking ? Quaternion.Euler(lookAroundRotation) : Quaternion.identity;

            activeUI.transform.position = Vector3.Lerp(activeUI.transform.position, targetPos, Time.deltaTime * lookAroundSpeed);
            activeUI.transform.rotation = Quaternion.Lerp(activeUI.transform.rotation, targetRot,Time.deltaTime * lookAroundSpeed);
        }
    }

    public void DisplayDilemma(Dilemma dilemma)
    {
        ClearUI();

        currentDilema = dilemma;
        votingUI.SetActive(true);
        votingHeader.text = dilemma.Title;
        descriptionText.text = dilemma.Description;
        yesText.text = dilemma.YesDescription;
        noText.text = dilemma.NoDescription;

        SlideIn(votingUI);
    }

    private void SubmitVote(int optionIndex)
    {
        if (votingLocked) return;

        int influenceSpent = int.TryParse(influenceField.text, out int inf) ? inf : 0;
        if (ClientResources.Instance.Influence < influenceSpent)
        {
            Debug.Log("Not enough influence!");
            signature.ClearSignature();
            return;
        }

        votingLocked = true;

        ClientResources.Instance.ModifyInfluence(-influenceSpent);
        NetworkManager.Client.Send(new CTS_VoteOnDilemma(optionIndex, influenceSpent));
        influenceField.interactable = false;

        SlideOut(votingUI);
        SlideIn(resultUI);
    }

    public void DisplayResult(STC_DilemmaResult result)
    {
        resultHeader.text = currentDilema.Title;
        
        resultText.text = "Winning Option: ";
        resultText.text += result.WinningOption == 0 ? currentDilema.YesDescription : currentDilema.NoDescription;
        ClientTracks.Instance.ApplyModifier(result.TrackModifier);
        resultBeingShown = true;
    }

    private void ClearUI()
    {
        resultTime = 0f;
        votingLocked = false;
        influenceField.interactable = true;
        influenceField.text = string.Empty;

        yesCheckBox.ClearSignature();
        noCheckBox.ClearSignature();
        signature.ClearSignature();

        votingUI.SetActive(false);
        resultUI.SetActive(false);

        currentDilema = null;
    }

    private void ExclusiveSignature(SignatureDrawer signedOne, SignatureDrawer other)
    {
        if (signedOne.OnSignatureComplete())
            other.ClearSignature();
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
            if (votingSlide != null) StopCoroutine(votingSlide);
            votingSlide = StartCoroutine(Slide(ui, true));
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
        }
        else
        {
            if (votingSlide != null) StopCoroutine(votingSlide);
            votingSlide = StartCoroutine(Slide(ui, false));
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
        // only digits
        return char.IsDigit(addedChar) ? addedChar : '\0';
    }
}