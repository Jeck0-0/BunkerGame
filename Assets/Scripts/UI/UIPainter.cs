using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class UIPainter : Singleton<UIPainter>
{
    [SerializeField] PainterSettings settings;
    public List<UIToPaint> uiToPaint;

    private Color prevColor1, prevColor2, prevColor3;

    private void Start()
    {
        if (settings == null)
        Debug.LogWarning("UIPainter: settings not assigned");
    }

    public void RegisterUIElement(UIToPaint ui)
    {
        if (ui == null || uiToPaint.Contains(ui)) return;
        uiToPaint.Add(ui);
        ApplyColor(ui);
    }

    public void UnregisterUIElement(UIToPaint ui)
    {
        if (ui == null || !uiToPaint.Contains(ui)) return;
        uiToPaint.Remove(ui);
    }

    private void Update()
    {
        if (settings == null || uiToPaint.Count == 0) return;

        for (int i = 0; i < uiToPaint.Count; i++)
        {
            var ui = uiToPaint[i];
            if (ui == null || ui.UiImage == null) continue;

            switch (ui.Type)
            {
                case UIToPaintType.Color1:
                    ui.UiImage.color = settings.Color1;
                    break;
                case UIToPaintType.Color2:
                    ui.UiImage.color = settings.Color2;
                    break;
                case UIToPaintType.Color3:
                    ui.UiImage.color = settings.Color3;
                    break;
                case UIToPaintType.Flicker:
                    UpdateFlicker(ui.UiImage);
                    break;
            }
        }

        prevColor1 = settings.Color1;
        prevColor2 = settings.Color2;
        prevColor3 = settings.Color3;
    }

    private void ApplyColor(UIToPaint ui)
    {
        if (ui.UiImage == null) return;

        switch (ui.Type)
        {
            case UIToPaintType.Color1:
                ui.UiImage.color = settings.Color1;
                break;
            case UIToPaintType.Color2:
                ui.UiImage.color = settings.Color2;
                break;
            case UIToPaintType.Color3:
                ui.UiImage.color = settings.Color3;
                break;
            case UIToPaintType.Flicker:
                ui.UiImage.color = settings.FlickerStartColor;
                break;
        }
    }

    private void UpdateFlicker(Image image)
    {
        float t = Mathf.PingPong(Time.time * settings.FlickerSpeed, 1f);
        image.color = Color.Lerp(settings.FlickerStartColor, settings.FlickerEndColor, t);
    }

    #if UNITY_EDITOR

    private void OnValidate()
    {
        if (!Application.isPlaying && settings != null)
        {
            foreach (var ui in FindObjectsByType<UIToPaint>(FindObjectsSortMode.None))
            ApplyColor(ui);
        }
    }

    #endif

}

public enum UIToPaintType
{
    Color1,
    Color2,
    Color3,
    Flicker
}

[CreateAssetMenu(fileName = "Painter Settings", menuName = "Scriptable Objects/Painter Settings")]
public class PainterSettings : ScriptableObject
{
    [Header("Basic Colors")]
    public Color Color1 = Color.white;
    public Color Color2 = Color.green;
    public Color Color3 = Color.blueViolet;

    [Header("Flicker")]
    public Color FlickerStartColor = Color.white;
    public Color FlickerEndColor = Color.whiteSmoke;
    public float FlickerSpeed = 2f;
}