using System.Collections;
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
    [SerializeField] Scrollbar backgroungAndSymbolScrollbar;

    [Header("Player Color")]
    [SerializeField] Color[] playerColors;
    [SerializeField] Button colorButtonPrefab;
    [SerializeField] Transform colorButtonParent;

    [Header("Emblem Color")]
    [SerializeField] Image emblemColorPreview;
    [SerializeField] Slider rSlider;
    [SerializeField] Slider gSlider;
    [SerializeField] Slider bSlider;

    [Header("Backgrounds Menu")]
    [SerializeField] Button backgroundButton;
    [SerializeField] Transform backgroundMenuParent;

    [Header("Symbols Menu")]
    [SerializeField] int maxSymbols = 5;
    [SerializeField] Button symbolButton;
    [SerializeField] Transform symbolMenuParent;

    [Header("Preview")]
    [SerializeField] Transform emblemPreviewParent;
    [SerializeField] Image[] previewLayers;
    [SerializeField] Transform symbolParent;
    [SerializeField] Sprite defaultSymbolSprite;
    [SerializeField] Button finishButton;

    [Header("Layer Controls")]
    [SerializeField] Transform LayerButtonParent;
    [SerializeField] Button LayerButton;

    [Header("Symbol Controls")]
    [SerializeField] GameObject symbolControlsOverlay;
    [SerializeField] Button moveLayerUpButton;
    [SerializeField] Button moveLayerDownButton;
    [SerializeField] Slider symbolPosXSlider;
    [SerializeField] Slider symbolPosYSlider;
    [SerializeField] Slider symbolScaleSlider;
    [SerializeField] Slider symbolRotationSlider;

    private BackgroundPattern activePattern;
    private List<Color> layerColors = new List<Color>();

    private List<SymbolLayer> symbolLayers = new List<SymbolLayer>();
    private int activeSymbolIndex = -1; 
    private int currentLayerIndex = 0;

    private Color playerColor = Color.white;

    // outlines for top menu buttons
    private Image backgroundButtonOutline;
    private Image symbolButtonOutline;
    private Image colorButtonOutline;

    private bool editingSymbol = false;
    private bool isSyncingSliders = false;
    private bool isRebuildingUI = false;
    private bool canFinishFaction = false;

    private void Start()
    {
        // slider ranges
        rSlider.maxValue = 0.9f;
        gSlider.maxValue = 0.9f;
        bSlider.maxValue = 0.9f;

        symbolPosXSlider.minValue = -150f;
        symbolPosXSlider.maxValue = 150f;
        symbolPosYSlider.minValue = -150f;
        symbolPosYSlider.maxValue = 150f;
        symbolScaleSlider.minValue = 0.5f;
        symbolScaleSlider.maxValue = 3.9f;
        symbolRotationSlider.minValue = -180f;
        symbolRotationSlider.maxValue = 180f;

        // initial slider values
        symbolPosXSlider.value = 0;
        symbolPosYSlider.value = 0;
        symbolScaleSlider.value = 1;
        symbolRotationSlider.value = 0;

        // player color buttons
        if (playerColors != null && colorButtonPrefab != null && colorButtonParent != null)
        {
            foreach (var pColor in playerColors)
            {
                var button = Instantiate(colorButtonPrefab, colorButtonParent);
                var btnImg = button.GetComponent<Image>();
                var preview = FindChildImage(button.transform, "ColorPreview") ?? GetFirstChildImage(button.transform);

                if (preview != null)
                preview.color = pColor;

                var outlineImage = btnImg;
                button.onClick.AddListener(() => SetPlayerColor(pColor, outlineImage));
            }
        }

        // menu outlines
        if (backgroundButton != null) backgroundButtonOutline = backgroundButton.GetComponent<Image>();
        if (symbolButton != null) symbolButtonOutline = symbolButton.GetComponent<Image>();

        // menu callbacks
        if (backgroundButton != null) backgroundButton.onClick.AddListener(BuildBackgroundMenu);
        if (symbolButton != null) symbolButton.onClick.AddListener(BuildSymbolMenu);
        if (finishButton != null) finishButton.onClick.AddListener(FinishFaction);
        if (finishButton != null) finishButton.interactable = false;

        // color slider callbacks
        rSlider.onValueChanged.AddListener(_ => UpdateActiveColor());
        gSlider.onValueChanged.AddListener(_ => UpdateActiveColor());
        bSlider.onValueChanged.AddListener(_ => UpdateActiveColor());
        nameInput.onValueChanged.AddListener(_ => CheckFactionRequirements());

        // symbol callbacks
        symbolPosXSlider.onValueChanged.AddListener(_ => UpdateSymbolTransform());
        symbolPosYSlider.onValueChanged.AddListener(_ => UpdateSymbolTransform());
        symbolScaleSlider.onValueChanged.AddListener(_ => UpdateSymbolTransform());
        symbolRotationSlider.onValueChanged.AddListener(_ => UpdateSymbolTransform());
        if (moveLayerUpButton != null) moveLayerUpButton.onClick.AddListener(MoveLayerUp);
        if (moveLayerDownButton != null) moveLayerDownButton.onClick.AddListener(MoveLayerDown);

        // initial menus
        if (availablePatterns != null && availablePatterns.Count > 0)
        SelectBackground(0);
        BuildSymbolMenu();
        BuildBackgroundMenu();
    }

    public void BuildBackgroundMenu()
    {
        if (backgroundMenuParent == null || menuButtonPrefab == null || availablePatterns == null)
        return;

        isRebuildingUI = true;
        ClearMenu(backgroundMenuParent);

        editingSymbol = false;
        symbolControlsOverlay.SetActive(true);
        SetSlidersInteractivity(false);

        if (symbolButtonOutline != null) symbolButtonOutline.enabled = false;
        if (backgroundButtonOutline != null) backgroundButtonOutline.enabled = true;

        for (int i = 0; i < availablePatterns.Count; i++)
        {
            var pattern = availablePatterns[i];
            var btnObj = Instantiate(menuButtonPrefab, backgroundMenuParent);
            var btn = btnObj.GetComponent<Button>();
            if (btn == null) continue;

            var childImages = GetChildImages(btnObj);
            // assign layer sprites
            for (int j = 0; j < childImages.Count; j++)
            {
                if (j == childImages.Count - 1) // last image is an overaly
                {
                    childImages[j].enabled = true;
                    continue;
                }
                if (j < pattern.Layers.Length)
                {
                    childImages[j].sprite = pattern.Layers[j];
                    childImages[j].enabled = true;

                    // default palette for menu preview
                    switch (j)
                    {
                        case 0: childImages[j].color = new Color(0.8f, 0.8f, 0.8f); break;
                        case 1: childImages[j].color = new Color(0.6f, 0.6f, 0.6f); break;
                        case 2: childImages[j].color = new Color(0.4f, 0.4f, 0.4f); break;
                        default: childImages[j].color = new Color(0.9f, 0.9f, 0.9f); break;
                    }
                }
                else
                {
                    childImages[j].enabled = false;
                }
            }

            int index = i;
            btn.onClick.AddListener(() =>
            {
                SelectBackground(index);
            });
        }

        BuildLayerButtons();
        backgroungAndSymbolScrollbar.value = 1;
        isRebuildingUI = false;
    }

    public void BuildSymbolMenu()
    {
        if (symbolMenuParent == null || menuButtonPrefab == null || availableSymbols == null)
        return;

        isRebuildingUI = true;
        ClearMenu(symbolMenuParent);

        editingSymbol = true;
        symbolControlsOverlay.SetActive(false);
        SetSlidersInteractivity(true);

        if (symbolButtonOutline != null) symbolButtonOutline.enabled = true;
        if (backgroundButtonOutline != null) backgroundButtonOutline.enabled = false;

        for (int i = 0; i < availableSymbols.Count; i++)
        {
            var symbol = availableSymbols[i];
            var btnObj = Instantiate(menuButtonPrefab, symbolMenuParent);
            var btn = btnObj.GetComponent<Button>();
            if (btn == null) continue;

            var childImages = GetChildImages(btnObj);
            foreach (var childImage in childImages)
            childImage.enabled = false;

            // first child used as icon
            if (childImages.Count > 0)
            {
                childImages[0].sprite = symbol.SymbolSprite;
                childImages[0].enabled = true;
            }

            // set outline
            if (childImages.Count > 1)
            {
                childImages[1].color = Color.white;
                childImages[1].enabled = true;
            }

            childImages[childImages.Count - 1].enabled = true; // last image is an overaly

            int index = i;
            btn.onClick.AddListener(() => ApplySymbolTemplateToActive(index));
        }

        BuildLayerButtons();
        backgroungAndSymbolScrollbar.value = 1;
        isRebuildingUI = false;
    }

    public void AddSymbol()
    {
        if (symbolParent == null) return;
        if (symbolLayers.Count >= maxSymbols) return;

        var go = new GameObject("SymbolLayer", typeof(Image));
        go.transform.SetParent(symbolParent, false);

        var img = go.GetComponent<Image>();
        img.sprite = defaultSymbolSprite;
        img.color = Color.white;
        var rt = img.rectTransform;
        rt.anchoredPosition = Vector2.zero;
        rt.localScale = Vector3.one;
        rt.localEulerAngles = Vector3.zero;

        var layer = new SymbolLayer
        {
            Image = img,
            Color = Color.white,
            Sprite = defaultSymbolSprite,
            Position = Vector3.zero,
            Scale = 1f,
            Rotation = 0f
        };

        symbolLayers.Add(layer);
        // select new symbol for editing
        activeSymbolIndex = symbolLayers.Count - 1;
        editingSymbol = true;

        BuildLayerButtons();
        UpdateLayerUI();
    }

    public void RemoveActiveSymbol()
    {
        if (activeSymbolIndex < 0 || activeSymbolIndex >= symbolLayers.Count) return;

        var s = symbolLayers[activeSymbolIndex];
        if (s.Image != null) Destroy(s.Image.gameObject);
        symbolLayers.RemoveAt(activeSymbolIndex);

        // adjust selected index
        if (symbolLayers.Count == 0) activeSymbolIndex = -1;
        else activeSymbolIndex = Mathf.Clamp(activeSymbolIndex - 1, 0, symbolLayers.Count - 1);

        BuildLayerButtons();
        UpdateLayerUI();
    }

    private void SelectBackground(int index)
    {
        if (availablePatterns == null || index < 0 || index >= availablePatterns.Count) return;

        editingSymbol = false;
        activePattern = availablePatterns[index];

        // apply sprites
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

        layerColors.Clear();
        for (int i = 0; i < activePattern.Layers.Length; i++)
        {
            Color defaultColor;
            switch (i)
            {
                case 0: defaultColor = new Color(0.8f, 0.8f, 0.8f); break;
                case 1: defaultColor = new Color(0.6f, 0.6f, 0.6f); break;
                case 2: defaultColor = new Color(0.4f, 0.4f, 0.4f); break;
                default: defaultColor = new Color(0.9f, 0.9f, 0.9f); break;
            }

            previewLayers[i].color = defaultColor;
            layerColors.Add(defaultColor);
        }

        currentLayerIndex = 0;
        BuildLayerButtons();
        UpdateLayerUI();
    }

    private void ApplySymbolTemplateToActive(int templateIndex)
    {
        if (availableSymbols == null || templateIndex < 0)
        return;

        var tpl = availableSymbols[templateIndex];

        // If no active symbol, create one first
        if (activeSymbolIndex < 0)
        {
            if (symbolLayers.Count >= maxSymbols)
            return;

            AddSymbol();
            activeSymbolIndex = symbolLayers.Count - 1;
        }

        var layer = symbolLayers[activeSymbolIndex];

        if (tpl.Blank)
        {
            RemoveActiveSymbol();
            return;
        }

        if (layer.Image != null)
        {
            layer.Image.enabled = true;
            layer.Image.sprite = tpl.SymbolSprite;
        }

        layer.Sprite = tpl.SymbolSprite;

        BuildLayerButtons();
        UpdateLayerUI();
    }

    private void BuildLayerButtons()
    {
        if (LayerButtonParent == null || LayerButton == null) return;

        ClearMenu(LayerButtonParent);

        if (!editingSymbol) // patterns
        {
            if (activePattern == null) return;

            for (int i = 0; i < activePattern.Layers.Length; i++)
            {
                int index = i;
                var btnObj = Instantiate(LayerButton.gameObject, LayerButtonParent);
                var btn = btnObj.GetComponent<Button>();
                var label = btnObj.GetComponentInChildren<TMP_Text>();
                var preview = FindChildImage(btnObj.transform, "ColorPreview") ?? GetFirstChildImage(btnObj.transform);

                if (label != null) label.text = "";
                if (preview != null) preview.color = layerColors[index];

                btn.onClick.AddListener(() =>
                {
                    currentLayerIndex = index;
                    UpdateLayerUI();
                    HighlightActiveLayerButton(index);
                });
            }

            currentLayerIndex = Mathf.Clamp(currentLayerIndex, 0, Mathf.Max(0, activePattern.Layers.Length - 1));
            DelayedHighlightActiveLayerButton(currentLayerIndex);
            UpdateLayerUI();
        }
        else // symbols
        {
            for (int i = 0; i < symbolLayers.Count; i++)
            {
                int index = i;
                var btnObj = Instantiate(LayerButton.gameObject, LayerButtonParent);
                var btn = btnObj.GetComponent<Button>();
                var label = btnObj.GetComponentInChildren<TMP_Text>();
                var preview = FindChildImage(btnObj.transform, "ColorPreview") ?? GetFirstChildImage(btnObj.transform);

                if (preview != null)
                {
                    preview.color = symbolLayers[i].Color;
                    preview.sprite = symbolLayers[i].Sprite;
                }

                btn.onClick.AddListener(() =>
                {
                    activeSymbolIndex = index;
                    UpdateLayerUI();
                    HighlightActiveLayerButton(index);
                    ApplySymbolToSliders(symbolLayers[activeSymbolIndex]);
                });
            }

            // Add button
            if (symbolLayers.Count < maxSymbols)
            {
                var addBtnObj = Instantiate(LayerButton.gameObject, LayerButtonParent);
                var addBtn = addBtnObj.GetComponent<Button>();
                var addLabel = addBtnObj.GetComponentInChildren<TMP_Text>();
                if (addLabel != null) addLabel.text = "+";
                addBtn.onClick.AddListener(AddSymbol);
            }

            if (symbolLayers.Count == 0) activeSymbolIndex = -1;
            else activeSymbolIndex = Mathf.Clamp(activeSymbolIndex, 0, symbolLayers.Count - 1);

            DelayedHighlightActiveLayerButton(activeSymbolIndex);
            UpdateLayerUI();
        }
    }

    private void HighlightActiveLayerButton(int activeIndex)
    {
        if (LayerButtonParent == null) return;

        for (int i = 0; i < LayerButtonParent.childCount; i++)
        {
            var child = LayerButtonParent.GetChild(i);
            var outlineTransform = child.Find("Outline");
            if (outlineTransform != null)
            {
                var outlineImage = outlineTransform.GetComponent<Image>();
                if (outlineImage != null)
                outlineImage.enabled = (i == activeIndex);
            }
        }
    }

    private void UpdateLayerUI()
    {
        if (editingSymbol)
        {
            if (activeSymbolIndex >= 0 && activeSymbolIndex < symbolLayers.Count)
            {
                var sym = symbolLayers[activeSymbolIndex];
                Color c = sym.Color;
                rSlider.value = c.r;
                gSlider.value = c.g;
                bSlider.value = c.b;
                emblemColorPreview.color = c;

                // set sliders to symbol's transform
                ApplySymbolToSliders(sym);
            }
            else
            {
                rSlider.value = 1f;
                gSlider.value = 1f;
                bSlider.value = 1f;
                emblemColorPreview.color = Color.white;
            }

            return;
        }

        // background mode
        if (activePattern == null || currentLayerIndex >= layerColors.Count) return;

        Color layerColor = layerColors[currentLayerIndex];
        rSlider.value = layerColor.r;
        gSlider.value = layerColor.g;
        bSlider.value = layerColor.b;
        emblemColorPreview.color = layerColor;
    }

    private void UpdateActiveColor()
    {
        if (isRebuildingUI) return;
        // wait for old buttons to be destroyed
        // because of the stupid unity index issues when destroying and Instantiating at the same frame

        Color newColor = new Color(rSlider.value, gSlider.value, bSlider.value);
        emblemColorPreview.color = newColor;

        if (!editingSymbol)
        {
            // background color change
            if (activePattern == null || currentLayerIndex >= previewLayers.Length) return;
            previewLayers[currentLayerIndex].color = newColor;
            layerColors[currentLayerIndex] = newColor;

            var previewBtnImg = FindChildImage(LayerButtonParent.GetChild(currentLayerIndex), "ColorPreview")
            ?? GetFirstChildImage(LayerButtonParent.GetChild(currentLayerIndex));
            if (previewBtnImg != null) previewBtnImg.color = newColor;
        }
        else
        {
            // symbol color change
            if (activeSymbolIndex < 0 || activeSymbolIndex >= symbolLayers.Count) return;

            var sym = symbolLayers[activeSymbolIndex];
            sym.Color = newColor;
            if (sym.Image != null) sym.Image.color = newColor;

            // update UI button preview
            var previewBtnImg = FindChildImage(LayerButtonParent.GetChild(activeSymbolIndex), "ColorPreview")
            ?? GetFirstChildImage(LayerButtonParent.GetChild(activeSymbolIndex));
            if (previewBtnImg != null) previewBtnImg.color = newColor;
        }
    }

    private void UpdateSymbolTransform()
    {
        if (isSyncingSliders) return;

        if (activeSymbolIndex < 0 || activeSymbolIndex >= symbolLayers.Count) return;

        var sym = symbolLayers[activeSymbolIndex];
        if (sym.Image == null) return;

        var rect = sym.Image.rectTransform;
        Vector2 newAnchored = new Vector2(symbolPosXSlider.value, symbolPosYSlider.value);
        rect.anchoredPosition = newAnchored;

        rect.localScale = Vector3.one * symbolScaleSlider.value;
        rect.localEulerAngles = new Vector3(0f, 0f, symbolRotationSlider.value);

        sym.Position = rect.anchoredPosition;
        sym.Scale = symbolScaleSlider.value;
        sym.Rotation = symbolRotationSlider.value;
    }

    private void ApplySymbolToSliders(SymbolLayer sym)
    {
        if (sym == null || sym.Image == null) return;

        isSyncingSliders = true;

        var rt = sym.Image.rectTransform;
        symbolPosXSlider.value = rt.anchoredPosition.x;
        symbolPosYSlider.value = rt.anchoredPosition.y;
        symbolScaleSlider.value = rt.localScale.x;

        float z = rt.localEulerAngles.z;
        if (z > 180f) z -= 360f;
        symbolRotationSlider.value = z;

        isSyncingSliders = false;
    }

    private void MoveLayerUp()
    {
        if (activeSymbolIndex < 0 || activeSymbolIndex >= symbolLayers.Count - 1 | !editingSymbol)
        return;

        // swap list entries
        var upperIndex = activeSymbolIndex + 1;
        var temp = symbolLayers[activeSymbolIndex];
        symbolLayers[activeSymbolIndex] = symbolLayers[upperIndex];
        symbolLayers[upperIndex] = temp;

        // swap hierarchy order
        var currentTransform = symbolLayers[upperIndex].Image.transform;
        currentTransform.SetSiblingIndex(currentTransform.GetSiblingIndex() + 1);

        // update index
        activeSymbolIndex = upperIndex;

        BuildLayerButtons();
        DelayedHighlightActiveLayerButton(activeSymbolIndex);
    }

    private void MoveLayerDown()
    {
        if (activeSymbolIndex <= 0 || activeSymbolIndex >= symbolLayers.Count | !editingSymbol)
        return;

        // swap list entries
        var lowerIndex = activeSymbolIndex - 1;
        var temp = symbolLayers[activeSymbolIndex];
        symbolLayers[activeSymbolIndex] = symbolLayers[lowerIndex];
        symbolLayers[lowerIndex] = temp;

        // swap hierarchy order
        var currentTransform = symbolLayers[lowerIndex].Image.transform;
        currentTransform.SetSiblingIndex(currentTransform.GetSiblingIndex() - 1);

        // update index
        activeSymbolIndex = lowerIndex;

        BuildLayerButtons();
        DelayedHighlightActiveLayerButton(activeSymbolIndex);
    }

    private void CheckFactionRequirements()
    {
        if (nameInput.text.Length > 0 && playerColor != Color.white)
        {
            finishButton.interactable = true;
            canFinishFaction = true;
        }
        else
        {
            finishButton.interactable = false;
            canFinishFaction = false;
        }
    }

    private void FinishFaction()
    {
        if (!canFinishFaction) return;

    }

    #region Utilities

    private void SetSlidersInteractivity(bool on)
    {
        moveLayerUpButton.interactable = on;
        moveLayerDownButton.interactable = on;
        symbolPosXSlider.interactable = on;
        symbolPosYSlider.interactable = on;
        symbolRotationSlider.interactable = on;
        symbolScaleSlider.interactable = on;
    }

    private Image FindChildImage(Transform parent, string childName)
    {
        if (parent == null) return null;
        var t = parent.Find(childName);
        if (t == null) return null;
        return t.GetComponent<Image>();
    }

    private Image GetFirstChildImage(Transform parent)
    {
        if (parent == null) return null;
        var imgs = parent.GetComponentsInChildren<Image>();
        foreach (var img in imgs)
        {
            if (img.transform != parent) // skip parent
           return img;
        }

        // parent if no child found
        return parent.GetComponent<Image>();
    }

    private List<Image> GetChildImages(GameObject root)
    {
        var list = new List<Image>();
        if (root == null) return list;

        var all = root.GetComponentsInChildren<Image>(true);
        foreach (var img in all)
        {
            if (img.gameObject == root) continue;
            list.Add(img);
        }
        return list;
    }

    private void ClearMenu(Transform parent)
    {
        if (parent == null) return;
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Destroy(parent.GetChild(i).gameObject);
        }
    }

    private void SetPlayerColor(Color color, Image outlineImage)
    {
        if (colorButtonOutline != null) colorButtonOutline.enabled = false;
        colorButtonOutline = outlineImage;
        playerColor = color;
        if (outlineImage != null) outlineImage.enabled = true;
        CheckFactionRequirements();
    }

    private void DelayedHighlightActiveLayerButton(int activeIndex) // Have to wait one frame for unity to instantiate the buttons
    {
        StartCoroutine(DelayedHighlight(activeIndex));
    }

    private IEnumerator DelayedHighlight(int activeIndex)
    {
        yield return null; // wait one frame
        // yield return new WaitForSecondsRealtime(0.1f); this may look better

        if (LayerButtonParent == null) yield break;

        for (int i = 0; i < LayerButtonParent.childCount; i++)
        {
            var child = LayerButtonParent.GetChild(i);
            var outlineTransform = child.Find("Outline");
            if (outlineTransform != null)
            {
                var outlineImage = outlineTransform.GetComponent<Image>();
                if (outlineImage != null)
                outlineImage.enabled = (i == activeIndex);
            }
        }
    }

    #endregion

    public EmblemData BuildEmblemPacket()
    {
        var packet = new EmblemData();

        packet.FactionName = nameInput.text;
        packet.PlayerColor = playerColor;

        // original scale
        packet.originalEmblemSize = (emblemPreviewParent as RectTransform).rect.size;

        // background
        packet.PatternID = activePattern != null ? activePattern.name : "";
        packet.LayerColors = new List<Color>(layerColors);

        // symbols
        foreach (var sym in symbolLayers)
        {
            var sp = new EmblemData.SymbolData
            {
                SymbolID = sym.Sprite != null ? sym.Sprite.name : "",
                Color = sym.Color,
                Position = sym.Position,
                Scale = sym.Scale,
                Rotation = sym.Rotation
            };

            packet.Symbols.Add(sp);
        }

        return packet;
    }
}

[System.Serializable]
public class SymbolLayer
{
    public Image Image;
    public Color Color = Color.white;
    public Sprite Sprite;
    public Vector3 Position;
    public float Scale = 1f;
    public float Rotation = 0f;
}