using Networking;
using Packets;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SecretObjectivelUI : Singleton<SecretObjectivelUI>
{
    private SecretObjective objective;

    [Header("Objective UI")]
    [SerializeField] GameObject UI;
    [SerializeField] GameObject trackIconPrefab;
    [SerializeField] TextMeshProUGUI objectiveHeader;
    [SerializeField] TextMeshProUGUI objectiveDescription;
    [SerializeField] Transform tracksParent;
    [SerializeField] GameObject positiveTrackPrefab;
    [SerializeField] GameObject neutralTrackPrefab;
    [SerializeField] GameObject negativeTrackPrefab;

    [Header("Resource Icons")]
    [SerializeField] Sprite orderSprite;
    [SerializeField] Sprite populationSprite;
    [SerializeField] Sprite foodSprite;
    [SerializeField] Sprite energySprite;
    [SerializeField] Sprite moraleSprite;

    [Header("Slide in/Out")]
    [SerializeField] float slideInDelay = 1f;
    [SerializeField] Transform tablePosition;
    [SerializeField] Vector3 offScreenPosition;
    [SerializeField] float slideDuration = 1.2f;
    [SerializeField] AnimationCurve slideCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] float delayBeforeRemoving = 1f;

    [Header("Look Around")]
    [SerializeField] Vector3 pauseOffset = new Vector3(0, -1.8f, -0.8f);
    [SerializeField] Vector3 pauseRotation = new Vector3(10f, 0f, 0f);
    [SerializeField] float lookAroundSpeed = 5f;

    [Header("Name Plate")]
    [SerializeField] YourNamePlate namePlate;

    private bool documentIsOut = false;
    private float showTime;

    protected override void Awake()
    {
        base.Awake();
        GameClient.Subscribe<STC_SecretObjective>(ReceiveObjective);
        GameClient.Subscribe<STC_GameStart>(GameStart);
    }

    private void OnDestroy()
    {
        GameClient.Unsubscribe<STC_SecretObjective>(ReceiveObjective);
        GameClient.Unsubscribe<STC_GameStart>(GameStart);
    }

    private void GameStart(BasePacket p) => SlideOut();

    public void ShowObjective()
    {
        SlideIn();
    }

    private void ReceiveObjective(BasePacket p)
    {
        var packet = (STC_SecretObjective)p;
        objective = Resources.Load<SecretObjective>("ScriptableObjects/SecretObjectives/" + packet.ObjectiveId);
        DisplaySecretObjective();
        SlideIn();
    }

    private void DisplaySecretObjective()
    {
        if (namePlate) namePlate.SetObjective(objective.RoleName);
        else Debug.LogWarning("Assign the nameplate to secret objective UI");

            TracksUI.Instance.SetObjective(objective);
        objectiveHeader.text = objective.RoleName;
        objectiveDescription.text = objective.Description;

        if (objective.PositiveTracks.Count > 0)
        {
            var positiveTrackParent = Instantiate(positiveTrackPrefab, tracksParent);
            SetIcons(objective.PositiveTracks, positiveTrackParent.transform);
        }
        if (objective.NeutralTracks.Count > 0)
        {
            var neutralTrackParent = Instantiate(neutralTrackPrefab, tracksParent);
            SetIcons(objective.NeutralTracks, neutralTrackParent.transform);
        }
        if (objective.NegativeTracks.Count > 0)
        {
            var negativeTrackParent = Instantiate(negativeTrackPrefab, tracksParent);
            SetIcons(objective.NegativeTracks, negativeTrackParent.transform);
        }
    }

    private void SetIcons(List<TrackType> tracks, Transform parent)
    {
        foreach (TrackType track in tracks)
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

            var trackObj = Instantiate(trackIconPrefab, parent);
            trackObj.GetComponent<Image>().sprite = trackSprite;
        }
    }

    private void Update()
    {
        if (documentIsOut) DocumentDown();

        if (Input.GetMouseButtonDown(0) && showTime >= delayBeforeRemoving)
        {
            SlideOut();
            showTime = 0f;
        }
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
        documentIsOut = true;

        StopAllCoroutines();
        StartCoroutine(Slide(UI, true));
    }

    private void SlideOut()
    {
        documentIsOut = false;
        StopAllCoroutines();
        StartCoroutine(Slide(UI, false));
    }

    private IEnumerator Slide(GameObject ui, bool visible)
    {
        if (visible)
        {
            new WaitForSecondsRealtime(slideInDelay);
            ui.SetActive(true);
        }

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
