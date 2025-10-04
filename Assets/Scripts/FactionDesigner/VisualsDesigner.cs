using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VisualsDesigner : MonoBehaviour
{
    [Header("Drop your designs here")]
    [SerializeField] List<BackgroundPattern> availablePatterns;
    [SerializeField] List<SymbolType> availableSymbols;

    [Header("UI References")]
    [SerializeField] TMP_InputField nameInput;
    [SerializeField] GameObject menuButtonPrefab;

    [Header("Player Color")]
    // TODO

    [Header("Emblem Color")]
    [SerializeField] Image emblemColorPreview;
    [SerializeField] Slider rSlider;
    [SerializeField] Slider gSlider;
    [SerializeField] Slider bSlider;

    [Header("Backgrounds Menu")]
    [SerializeField] Button backgroundButton;
    [SerializeField] Transform backgroundMenuParent;

    [Header("Symbols Menu")]
    [SerializeField] Button symbolButton;
    [SerializeField] Transform symbolMenuParent;

    [Header("Preview")]
    [SerializeField] Transform emblemPreviewParent;
    [SerializeField] Image[] previewLayers;
    [SerializeField] Image symbolImage;

    [Header("Layer Controls")]
    [SerializeField] Transform LayerButtonParent;
    [SerializeField] Button LayerButton;

    [Header("Symbol Transform Controls")]
    [SerializeField] Slider symbolPosXSlider;
    [SerializeField] Slider symbolPosYSlider;
    [SerializeField] Slider symbolScaleSlider;
    [SerializeField] Slider symbolRotationSlider;

    private BackgroundPattern activePattern;
    private int currentLayerIndex;
    private List<Color> layerColors = new List<Color>();

    private Color playerColor = Color.white;
    private FactionData factionData;

    private void Start()
    {

        if (symbolImage != null)
        {
            var rect = symbolImage.rectTransform;
            symbolPosXSlider.minValue = -100f;
            symbolPosXSlider.maxValue = 100f;
            symbolPosYSlider.minValue = -100f;
            symbolPosYSlider.maxValue = 100f;
            symbolScaleSlider.minValue = 0.5f;
            symbolScaleSlider.maxValue = 2f;
            symbolRotationSlider.minValue = 0f;
            symbolRotationSlider.maxValue = 360f;

            symbolPosXSlider.value = rect.anchoredPosition.x;
            symbolPosYSlider.value = rect.anchoredPosition.y;
            symbolScaleSlider.value = rect.localScale.x;
            symbolRotationSlider.value = rect.localEulerAngles.z;
        }


        rSlider.onValueChanged.AddListener(_ => UpdateActiveColor());
        gSlider.onValueChanged.AddListener(_ => UpdateActiveColor());
        bSlider.onValueChanged.AddListener(_ => UpdateActiveColor());

        backgroundButton.onClick.AddListener(() => BuildBackgroundMenu()); 
        symbolButton.onClick.AddListener(() => BuildSymbolMenu());

        symbolPosXSlider.onValueChanged.AddListener(_ => UpdateSymbolTransform());
        symbolPosYSlider.onValueChanged.AddListener(_ => UpdateSymbolTransform());
        symbolScaleSlider.onValueChanged.AddListener(_ => UpdateSymbolTransform());
        symbolRotationSlider.onValueChanged.AddListener(_ => UpdateSymbolTransform());

        // Default background
        if (availablePatterns.Count > 0) SelectBackground(0);
    }

    private void BuildBackgroundMenu()
    {
        ClearMenu(backgroundMenuParent);

        for (int i = 0; i < availablePatterns.Count; i++)
        {
            var pattern = availablePatterns[i];
            var btnObj = Instantiate(menuButtonPrefab, backgroundMenuParent);
            var btn = btnObj.GetComponent<Button>();
            var images = btnObj.GetComponentsInChildren<Image>();


            for (int j = 0; j < images.Length; j++)
            {
                if (j < pattern.Layers.Length)
                {
                    images[j].sprite = pattern.Layers[j];
                    images[j].enabled = true;

                    // default palette
                    switch (j)
                    {
                        case 0: images[j].color = Color.white; break;
                        case 1: images[j].color = Color.gray; break;
                        case 2: images[j].color = Color.black; break;
                        default: images[j].color = Color.white; break;
                    }
                }
                else
                {
                    images[j].enabled = false;
                }
            }

            int index = i;
            btn.onClick.AddListener(() => SelectBackground(index));
        }
    }

    private void BuildSymbolMenu()
    {
        ClearMenu(symbolMenuParent);

        for (int i = 0; i < availableSymbols.Count; i++)
        {
            var symbol = availableSymbols[i];
            var btnObj = Instantiate(menuButtonPrefab, symbolMenuParent);
            var btn = btnObj.GetComponent<Button>();
            var images = btnObj.GetComponentsInChildren<Image>();
            for (int j = 0; j < images.Length; j++)
            images[j].enabled = false;

            images[0].sprite = symbol.SymbolSprite;
            images[0].enabled = true;

            int index = i;
            btn.onClick.AddListener(() => SelectSymbol(index));
        }
    }

    private void SelectBackground(int index)
    {
        activePattern = availablePatterns[index];

        for (int i = 0; i < previewLayers.Length; i++)
        {
            if (i < activePattern.Layers.Length)
            {
                previewLayers[i].sprite = activePattern.Layers[i];
                previewLayers[i].enabled = true;
            }
            else
            {
                previewLayers[i].enabled = false;
            }
        }

        // Reset colors with default palette
        layerColors.Clear();
        for (int i = 0; i < activePattern.Layers.Length; i++)
        {
            Color defaultColor;
            switch (i)
            {
                case 0: defaultColor = Color.white; break;
                case 1: defaultColor = Color.gray; break;
                case 2: defaultColor = Color.black; break;
                default: defaultColor = Color.white; break;
            }

            previewLayers[i].color = defaultColor;
            layerColors.Add(defaultColor);
        }

        BuildLayerButtons();

        currentLayerIndex = 0;
        UpdateLayerUI();
    }
    private void BuildLayerButtons()
    {
        ClearMenu(LayerButtonParent);

        for (int i = 0; i < activePattern.Layers.Length; i++)
        {
            int index = i;
            var btnObj = Instantiate(LayerButton, LayerButtonParent);
            var btn = btnObj.GetComponent<Button>();
            var txt = btnObj.GetComponentInChildren<TMP_Text>();
            txt.text = $"Layer {i + 1}";

            btn.onClick.AddListener(() =>
            {
                currentLayerIndex = index;
                UpdateLayerUI();
                HighlightActiveLayerButton(index);
            });
        }

        // for symbol
        var symbolBtnObj = Instantiate(LayerButton, LayerButtonParent);
        var symbolBtn = symbolBtnObj.GetComponent<Button>();
        var symbolTxt = symbolBtnObj.GetComponentInChildren<TMP_Text>();
        symbolTxt.text = "Symbol";

        symbolBtn.onClick.AddListener(() =>
        {
            currentLayerIndex = -1; // special index for symbol
            UpdateLayerUI();
            HighlightActiveLayerButton(-1);
        });

        HighlightActiveLayerButton(0);
    }

    private void HighlightActiveLayerButton(int activeIndex)
    {
        for (int i = 0; i < LayerButtonParent.childCount; i++)
        {
            var btn = LayerButtonParent.GetChild(i).GetComponent<Button>();
            var colors = btn.colors;

            if (i == activeIndex || (activeIndex == -1 && i == LayerButtonParent.childCount - 1))
            {
                colors.normalColor = new Color(0.8f, 0.8f, 1f); // light blue
            }
            else
            {
                colors.normalColor = Color.white;
            }

            btn.colors = colors;
        }
    }


    private void SelectSymbol(int index)
    {
        symbolImage.sprite = availableSymbols[index].SymbolSprite;
    }

    private void UpdateLayerUI()
    {
        if (currentLayerIndex == -1) // for symbol
        {
            Color c = symbolImage.color;
            rSlider.value = c.r;
            gSlider.value = c.g;
            bSlider.value = c.b;
            emblemColorPreview.color = c;
            return;
        }

        if (activePattern == null || currentLayerIndex >= layerColors.Count) return;

        Color layerColor = layerColors[currentLayerIndex];
        rSlider.value = layerColor.r;
        gSlider.value = layerColor.g;
        bSlider.value = layerColor.b;
        emblemColorPreview.color = layerColor;
    }

    private void UpdateActiveColor()
    {
        Color newColor = new Color(rSlider.value, gSlider.value, bSlider.value);
        emblemColorPreview.color = newColor;

        if (currentLayerIndex == -1) // for symbol
        {
            symbolImage.color = newColor;
            return;
        }

        if (activePattern == null || currentLayerIndex >= previewLayers.Length) return;

        previewLayers[currentLayerIndex].color = newColor;
        layerColors[currentLayerIndex] = newColor;
    }


    private void ClearMenu(Transform parent)
    {
        foreach (Transform child in parent) Destroy(child.gameObject);
    }

    private void UpdateSymbolTransform()
    {
        if (symbolImage == null) return;

        RectTransform rect = symbolImage.rectTransform;

        // Apply values from sliders
        Vector2 newPos = new Vector2(symbolPosXSlider.value, symbolPosYSlider.value);
        rect.anchoredPosition = newPos;

        float scaleValue = symbolScaleSlider.value;
        rect.localScale = new Vector3(scaleValue, scaleValue, 1f);

        float rotationValue = symbolRotationSlider.value;
        rect.localEulerAngles = new Vector3(0f, 0f, rotationValue);
    }

    public FactionData BuildFaction()
    {
        factionData = new FactionData
        {
            FactionName = nameInput.text,
            PlayerColor = playerColor,

            BackgroundId = availablePatterns.IndexOf(activePattern),
            BackgroundColors = layerColors.ToArray(),

            SymbolId = availableSymbols.FindIndex(s => s.SymbolSprite == symbolImage.sprite),
            SymbolColor = symbolImage.color,
            SymbolPosition = symbolImage.rectTransform.anchoredPosition,
            SymbolScale = symbolImage.rectTransform.localScale.x,
            SymbolRotation = symbolImage.rectTransform.localEulerAngles.z
        };

        return factionData;
    }
}