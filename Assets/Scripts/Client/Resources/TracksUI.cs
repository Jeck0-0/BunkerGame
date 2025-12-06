using Client;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TrackUI : Singleton<TrackUI>
{
    [SerializeField] List<TrackUIElement> trackReferences;
    private Dictionary<TrackType, TrackUIElement> tracks = new();

    void Start()
    {
        tracks = new Dictionary<TrackType, TrackUIElement>();

        foreach (var track in trackReferences)
        {
            tracks.Add(track.type, track);

            track.slider.minValue = 0;
            track.slider.maxValue = ClientTracks.Instance.maxValue;

            track.upIcon.SetActive(false);
            track.neutralIcon.SetActive(false);
            track.downIcon.SetActive(false);
        }

        ClientTracks.Instance.ResourceReachedZero += OnTrackZero;

        UpdateAllTracks();
    }

    private void OnDisable()
    {
        if (ClientTracks.Instance != null)
            ClientTracks.Instance.ResourceReachedZero -= OnTrackZero;
    }

    public void SetObjective(SecretObjective objective)
    {
        foreach (var track in objective.PositiveTracks)
        {
            if (tracks.TryGetValue(track, out var element))
                element.upIcon.SetActive(true);
        }

        foreach (var track in objective.NeutralTracks)
        {
            if (tracks.TryGetValue(track, out var element))
                element.neutralIcon.SetActive(true);
        }

        foreach (var track in objective.NegativeTracks)
        {
            if (tracks.TryGetValue(track, out var element))
                element.downIcon.SetActive(true);
        }
    }

    public void UpdateAllTracks()
    {
        foreach (var type in tracks.Keys)
            UpdateTrack(type);
    }

    public void UpdateTrack(TrackType type)
    {
        if (!tracks.TryGetValue(type, out var track)) return;

        int value = ClientTracks.Instance.GetTrackValue(type);
        track.slider.value = value;
        track.label.text = value.ToString();
    }

    private void OnTrackZero(TrackType type)
    {
        UpdateTrack(type);
        Debug.LogWarning($"{type} reached zero");
    }
}
[System.Serializable]
public class TrackUIElement
{
    public TrackType type;

    public Slider slider;
    public TextMeshProUGUI label;
    public GameObject upIcon;
    public GameObject neutralIcon;
    public GameObject downIcon;
}