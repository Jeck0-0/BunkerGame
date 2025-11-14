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

    private bool resultBeingShown = false;
    private bool votingLocked;
    private float resultTime;

    private Coroutine votingSlide;
    private Coroutine resultSlide;

    void Start()
    {
        influenceField.contentType = TMP_InputField.ContentType.Custom;
        influenceField.onValidateInput += OnlyDigits;
        votingUI.SetActive(false);
        resultUI.SetActive(false);

        yesCheckBox.OnSigned += () => ExclusiveSignature(yesCheckBox, noCheckBox);
        noCheckBox.OnSigned += () => ExclusiveSignature(noCheckBox, yesCheckBox);
    }
    private void Update()
    {
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

    public void DisplayDilemma(Dilemma dilemma)
    {
        ClearUI();

        votingUI.SetActive(true);
        votingHeader.text = dilemma.Title;
        resultHeader.text = dilemma.Title;
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
        resultText.text = $"Winning Option: {result.WinningOption}";
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
    }

    private void ExclusiveSignature(SignatureDrawer signedOne, SignatureDrawer other)
    {
        if (signedOne.OnSignatureComplete())
            other.ClearSignature();
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
            if (votingSlide != null) StopCoroutine(votingSlide);
            votingSlide = StartCoroutine(Slide(ui, true));
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
            if (votingSlide != null) StopCoroutine(votingSlide);
            votingSlide = StartCoroutine(Slide(ui, false));
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

    private char OnlyDigits(string text, int charIndex, char addedChar)
    {
        // only digits
        return char.IsDigit(addedChar) ? addedChar : '\0';
    }
}