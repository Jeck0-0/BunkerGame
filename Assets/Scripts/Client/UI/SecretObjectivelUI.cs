using Networking;
using Packets;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SecretObjectivelUI : MonoBehaviour
{
    private SecretObjective objective;

    [Header("Objective UI")]
    [SerializeField] GameObject UI;
    [SerializeField] GameObject trackIconPrefab;
    [SerializeField] TextMeshProUGUI objectiveHeader;
    [SerializeField] TextMeshProUGUI objectiveDescription;
    [SerializeField] Transform positiveTracksParent;
    [SerializeField] Transform negativeTracksParent;
    [SerializeField] SignatureDrawer signature;

    [Header("Resource Icons")]
    [SerializeField] Sprite orderSprite;
    [SerializeField] Sprite populationSprite;
    [SerializeField] Sprite foodSprite;
    [SerializeField] Sprite energySprite;
    [SerializeField] Sprite moraleSprite;

    [Header("Slide in/Out")]
    [SerializeField] Transform tablePosition;
    [SerializeField] Vector3 offScreenPosition;
    [SerializeField] float slideDuration = 1.2f;
    [SerializeField] AnimationCurve slideCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Look Around")]
    [SerializeField] Vector3 pauseOffset = new Vector3(0, -1.8f, -0.8f);
    [SerializeField] Vector3 pauseRotation = new Vector3(10f, 0f, 0f);
    [SerializeField] float lookAroundSpeed = 5f;

    private bool disable = false;

    private void Awake()
    {
        NetworkManager.Client.Subscribe<STC_SecretObjective>(ReceiveObjective);
        NetworkManager.Client.Subscribe<STC_GameStart>(GameStart);
    }

    private void OnDestroy()
    {
        NetworkManager.Client?.Unsubscribe<STC_SecretObjective>(ReceiveObjective);
        NetworkManager.Client?.Unsubscribe<STC_GameStart>(GameStart);
    }

    private void GameStart(BasePacket p) => SlideOut();

    private void ReceiveObjective(BasePacket p)
    {
        var packet = (STC_SecretObjective)p;
        objective = Resources.Load<SecretObjective>("ScriptableObjects/SecretObjectives/" + packet.ObjectiveId);
        DisplaySecretObjective();
        SlideIn();
    }

    private void DisplaySecretObjective()
    {
        TrackUI.Instance.SetObjective(objective);
        objectiveHeader.text = objective.RoleName;
        objectiveDescription.text = objective.Description;

        foreach (TrackType track in objective.PositiveTracks)
        {
            Sprite trackSprite = orderSprite;

            switch (track)
            {
                case TrackType.Order:
                    trackSprite = orderSprite;
                    break;
                case TrackType.Population:
                    trackSprite = populationSprite;
                    break;
                case TrackType.Food:
                    trackSprite = foodSprite;
                    break;
                case TrackType.Energy:
                    trackSprite = energySprite;
                    break;
                case TrackType.Moral:
                    trackSprite = moraleSprite;
                    break;
            }

            var trackObj = Instantiate(trackIconPrefab, positiveTracksParent);
            trackObj.GetComponent<Image>().sprite = trackSprite;
        }

        foreach (TrackType track in objective.NegativeTracks)
        {
            Sprite trackSprite = orderSprite;

            switch (track)
            {
                case TrackType.Order:
                    trackSprite = orderSprite;
                    break;
                case TrackType.Population:
                    trackSprite = populationSprite;
                    break;
                case TrackType.Food:
                    trackSprite = foodSprite;
                    break;
                case TrackType.Energy:
                    trackSprite = energySprite;
                    break;
                case TrackType.Moral:
                    trackSprite = moraleSprite;
                    break;
            }

            var trackObj = Instantiate(trackIconPrefab, negativeTracksParent);
            trackObj.GetComponent<Image>().sprite = trackSprite;
        }
    }

    private void Update()
    {
        if (disable) return;
        DocumentDown();

        // if (ComputerManager.Instance.GetComputerUp()) return;
        // if (signature.OnSignatureComplete()) SubmitDocument();
    }

    private void SubmitDocument()
    {
        SlideOut();
    }

    private void DocumentDown()
    {
        Vector3 targetPos;
        Quaternion targetRot;

        if (ComputerManager.Instance.GetComputerUp())
        {
            targetPos = tablePosition.position + pauseOffset;
            targetRot = Quaternion.Euler(pauseRotation);
        }
        else
        {
            targetPos = tablePosition.position;
            targetRot = Quaternion.identity;
        }

        UI.transform.position = Vector3.Lerp(UI.transform.position, targetPos, Time.deltaTime * lookAroundSpeed);
        UI.transform.rotation = Quaternion.Lerp(UI.transform.rotation, targetRot, Time.deltaTime * lookAroundSpeed);
    }

    private void SlideIn()
    {
        UI.transform.rotation = Quaternion.identity;

        StopAllCoroutines();
        StartCoroutine(Slide(UI, true));
    }

    private void SlideOut()
    {
        disable = true;

        StopAllCoroutines();
        StartCoroutine(Slide(UI, false));
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
    }
}
