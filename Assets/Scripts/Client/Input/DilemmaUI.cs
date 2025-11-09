using System.Collections;
using Client;
using Networking;
using Packets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DilemmaUI : Singleton<DilemmaUI>
{
    [SerializeField] GameObject UI;

    [Header("Slide in/Out")]
    [SerializeField] Transform tablePosition;
    [SerializeField] Vector3 offScreenPosition;
    [SerializeField] float slideDuration = 1.2f;
    [SerializeField] AnimationCurve slideCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("UI References")]
    [SerializeField] TextMeshProUGUI titleText;
    [SerializeField] TextMeshProUGUI descriptionText;
    [SerializeField] TMP_InputField influenceField;
    [SerializeField] Button yesButton;
    [SerializeField] Button noButton;


    private bool votingLocked;
    private Coroutine slide;

    void Start()
    {
        yesButton.onClick.AddListener(() => SubmitVote(0));
        noButton.onClick.AddListener(() => SubmitVote(1));
        influenceField.contentType = TMP_InputField.ContentType.IntegerNumber;
        UI.SetActive(false);
    }

    public void DisplayDilemma(Dilemma dilemma)
    {
        UI.SetActive(true);
        titleText.text = dilemma.Title;
        descriptionText.text = dilemma.Description;
        SlideIn();
        votingLocked = false;
    }

    private void SubmitVote(int optionIndex)
    {
        if (votingLocked) return;

        int influenceSpent = int.TryParse(influenceField.text, out int inf) ? inf : 0;
        if (ClientResources.Instance.Influence < influenceSpent)
        {
            Debug.Log("Not enough influence!");
            return;
        }

        votingLocked = true;

        ClientResources.Instance.ModifyInfluence(-influenceSpent);
        NetworkManager.Client.Send(new CTS_VoteOnDilemma(optionIndex, influenceSpent));

        yesButton.interactable = false;
        noButton.interactable = false;
        influenceField.interactable = false;
    }

    public void DisplayResult(STC_DilemmaResult result)
    {
        ClientTracks.Instance.ApplyModifier(result.TrackModifier);
        descriptionText.text += $"Winning Option: {result.WinningOption}";
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