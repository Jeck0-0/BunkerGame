using Client;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TrackUI : Singleton<TrackUI>
{
    [Header("Sliders")]
    [SerializeField] Slider energySlider;
    [SerializeField] Slider foodSlider;
    [SerializeField] Slider moralSlider;
    [SerializeField] Slider orderSlider;
    [SerializeField] Slider populationSlider;

    [Header("Text fields")]
    [SerializeField] TextMeshProUGUI energyText;
    [SerializeField] TextMeshProUGUI foodText;
    [SerializeField] TextMeshProUGUI moralText;
    [SerializeField] TextMeshProUGUI orderText;
    [SerializeField] TextMeshProUGUI populationText;

    private Dictionary<TrackType, (Slider slider, TextMeshProUGUI label)> tracks;

    void Start()
    {
        tracks = new Dictionary<TrackType, (Slider, TextMeshProUGUI)>
        {
            { TrackType.Energy, (energySlider, energyText) },
            { TrackType.Food, (foodSlider, foodText) },
            { TrackType.Moral, (moralSlider, moralText) },
            { TrackType.Order, (orderSlider, orderText) },
            { TrackType.Population, (populationSlider, populationText) }
        };

        int startValue = ClientTracks.Instance.startValue;
        int maxValue = ClientTracks.Instance.maxValue;

        foreach (var slider in tracks.Values)
        {
            slider.slider.minValue = 0;
            slider.slider.maxValue = ClientTracks.Instance.maxValue;
        }

        ClientTracks.Instance.ResourceReachedZero += OnTrackZero;
        UpdateAllTracks();
    }

    private void OnDisable()
    {
        if (ClientTracks.Instance != null)
            ClientTracks.Instance.ResourceReachedZero -= OnTrackZero;
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