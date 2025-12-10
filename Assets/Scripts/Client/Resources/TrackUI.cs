using System;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Client
{
    public class TrackUI : MonoBehaviour
    {
        public TrackType type;
        
        [FoldoutGroup("References")] public Slider frontSlider;
        [FoldoutGroup("References")] public Slider backSlider;
        [FoldoutGroup("References")] public Image frontSliderFill;
        [FoldoutGroup("References")] public Image backSliderFill;
        [FoldoutGroup("References")] public TextMeshProUGUI currentValueText;
        [FoldoutGroup("References")] public TextMeshProUGUI diffText;
        [FoldoutGroup("References")] public TextMeshProUGUI trackNameText;
        [FoldoutGroup("References")] public GameObject upIcon;
        [FoldoutGroup("References")] public GameObject neutralIcon;
        [FoldoutGroup("References")] public GameObject downIcon;
        [FoldoutGroup("References")] public Image upRange;
        [FoldoutGroup("References")] public Image neutralRange;
        [FoldoutGroup("References")] public Image downRange;

        [FoldoutGroup("Colors")] public Color neutralColor;
        [FoldoutGroup("Colors")] public Color positiveColor;
        [FoldoutGroup("Colors")] public Color negativeColor;
        
        private int lastValue = -1;
        private void Awake()
        {
            backSlider.minValue = 0;
            backSlider.maxValue = ClientTracks.Instance.maxValue;
            frontSlider.minValue = 0;
            frontSlider.maxValue = ClientTracks.Instance.maxValue;
            
            upIcon.SetActive(false);
            neutralIcon.SetActive(false);
            downIcon.SetActive(false);
            
            trackNameText.text = type.ToString();
        }

        private void Start()
        {
            UpdateValue();
        }

        /// <summary>
        /// 0 for negative, 1 for neutral, 2 for positive
        /// </summary>
        public void SetObjective(int objective)
        {
            switch (objective)
            {
                case 0:
                    downIcon.SetActive(true);
                    downRange.gameObject.SetActive(true); 
                    break;
                case 1: 
                    neutralIcon.SetActive(true); 
                    neutralRange.gameObject.SetActive(true); 
                    break;
                case 2: 
                    upIcon.SetActive(true); 
                    upRange.gameObject.SetActive(true); 
                    break;
            }
        }

        public void UpdateValue()
        {
            int value = ClientTracks.Instance.GetTrackValue(type);
            if(lastValue == -1)
                lastValue = value;
            
            var diff = value - lastValue;
            lastValue = value;
            
            frontSlider.value = value;
            
            currentValueText.text = value.ToString();
            diffText.text = diff.ToString();
            
            diffText.gameObject.SetActive(true);
            if(diff == 0)
                diffText.gameObject.SetActive(false);
            else if(diff > 0)
                diffText.color = positiveColor;
            else 
                diffText.color = negativeColor;
            
            
            /*if (diff <= 0)
            {
                backSlider.value = lastValue;
                backSliderFill.color = negativeColor;
                frontSlider.value = value;
                frontSliderFill.color = neutralColor;
            }
            else
            {
                backSlider.value = value;
                backSliderFill.color = positiveColor;
                frontSlider.value = lastValue;
                frontSliderFill.color = neutralColor;
            }*/
        }
    }
}