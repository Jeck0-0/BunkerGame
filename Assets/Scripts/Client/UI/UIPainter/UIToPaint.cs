using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIToPaint : MonoBehaviour
{
    public UIToPaintType Type = UIToPaintType.Color1;

    private Image UiImage;
    private TextMeshProUGUI UiText;

    private void Awake()
    {
        UiImage = GetComponent<Image>();
        UiText = GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        if (UIPainter.Instance != null)
        UIPainter.Instance.RegisterUIElement(this);
    }

    private void OnDestroy()
    {
        if (UIPainter.Instance != null)
        UIPainter.Instance.UnregisterUIElement(this);
    }

    public void SetColor(Color color)
    {
        if (UiImage != null)
        UiImage.color = color;
        if (UiText != null)
        UiText.color = color;
    }
}
