using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EmblemBuilder : MonoBehaviour
{
    [Header("Prefabs & References")]
    [SerializeField] GameObject emblemUIPrefab;
    [SerializeField] Canvas worldCanvasPrefab;

    [Header("Resources")]
    [SerializeField] List<BackgroundPattern> availablePatterns;
    [SerializeField] List<SymbolType> availableSymbols;

    // Build an emblem inside existing UI
    public GameObject BuildUIEmblem(EmblemData packet, Transform parent, float scale = 1f)
    {
        if (packet == null || emblemUIPrefab == null)
        return null;

        var root = Instantiate(emblemUIPrefab, parent);
        ApplyEmblemToRoot(root, packet);
        return root;
    }

    // Build a world-space emblem
    public GameObject BuildWorldEmblem(EmblemData packet, Transform attachTo, Vector3 offset = default, float scale = 1f)
    {
        if (packet == null || worldCanvasPrefab == null || emblemUIPrefab == null)
        return null;

        var canvas = Instantiate(worldCanvasPrefab);
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;

        var rect = canvas.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(scale, scale);
        rect.position = attachTo.position + offset;
        rect.rotation = Quaternion.identity;

        var emblem = Instantiate(emblemUIPrefab, canvas.transform);
        ApplyEmblemToRoot(emblem, packet);

        return canvas.gameObject;
    }

    private void ApplyEmblemToRoot(GameObject root, EmblemData packet)
    {
        var bgParent = root.transform.Find("BackgroundParent");
        var symbolParent = root.transform.Find("SymbolParent");
        var playerOverlay = root.transform.Find("PlayerColorOverlay")?.GetComponent<Image>();

        if (bgParent == null || symbolParent == null)
        {
            Debug.LogWarning("Prefab misspealed, just like this warning");
            return;
        }


        // player color overlay 
        if (playerOverlay != null)
        playerOverlay.color = packet.PlayerColor;


        // background pattern
        BackgroundPattern pattern = null;
        if (!string.IsNullOrEmpty(packet.PatternID) && availablePatterns != null)
        {
            pattern = availablePatterns.Find(p => p.name == packet.PatternID);
        }
        // if not found use default pattern
        if (pattern == null && availablePatterns != null && availablePatterns.Count > 0)
        pattern = availablePatterns[0];

        int requiredLayers = (pattern != null && pattern.Layers != null) ? pattern.Layers.Length : 0;

        for (int i = 0; i < requiredLayers; i++)
        {
            Transform child = bgParent.Find($"Layer_{i}");
            if (child == null)
            {
                // create new GameObject with Image and proper anchors
                var obj = new GameObject($"Layer_{i}", typeof(RectTransform), typeof(Image));
                var rt = obj.GetComponent<RectTransform>();
                rt.SetParent(bgParent, false);
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.sizeDelta = Vector2.zero;
                rt.anchoredPosition = Vector2.zero;
                var img = obj.GetComponent<Image>();
                img.raycastTarget = false;
            }
        }

        // Disable any extra Layer
        for (int i = 0; i < bgParent.childCount; i++)
        {
            var child = bgParent.GetChild(i);
            if (!child.name.StartsWith("Layer_")) continue; 

            string idxStr = child.name.Substring("Layer_".Length);
            if (int.TryParse(idxStr, out int idx))
            {
                child.gameObject.SetActive(idx < requiredLayers);
            }
        }

        for (int i = 0; i < requiredLayers; i++)
        {
            var child = bgParent.Find($"Layer_{i}");
            if (child == null) continue;
            var img = child.GetComponent<Image>();
            if (img == null) continue;

            img.sprite = pattern.Layers[i];
            img.enabled = true;

            Color colorToUse = Color.white;
            if (packet.LayerColors != null && i < packet.LayerColors.Count)
            {
                colorToUse = packet.LayerColors[i];
            }
            else
            {
                // default palete
                switch (i)
                {
                    case 0: colorToUse = new Color(0.9f, 0.9f, 0.9f); break;
                    case 1: colorToUse = Color.gray; break;
                    case 2: colorToUse = Color.black; break;
                    default: colorToUse = new Color(0.9f, 0.9f, 0.9f); break;
                }
            }

            img.color = colorToUse;
        }

        // symbol layers
        if (packet.Symbols != null && packet.Symbols.Count > 0)
        {
            foreach (var symbol in packet.Symbols)
            {
                // find symbol
                Sprite sprite = null;
                if (availableSymbols != null)
                {
                    var tpl = availableSymbols.Find(s => s.SymbolSprite != null && s.SymbolSprite.name == symbol.SymbolID);
                    if (tpl != null) sprite = tpl.SymbolSprite;
                }

                var obj = new GameObject($"Symbol_{symbol.SymbolID}", typeof(RectTransform), typeof(Image));
                obj.transform.SetParent(symbolParent, false);
                var img = obj.GetComponent<Image>();
                img.raycastTarget = false;
                img.sprite = sprite;
                img.color = symbol.Color;

                var rt = img.rectTransform;
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = symbol.Position;
                rt.localScale = Vector3.one * symbol.Scale;
                rt.localEulerAngles = new Vector3(0f, 0f, symbol.Rotation);
            }
        }

        // update layout
        var layout = root.GetComponentInChildren<LayoutGroup>();
        if (layout != null) LayoutRebuilder.ForceRebuildLayoutImmediate(root.GetComponent<RectTransform>());
    }
}