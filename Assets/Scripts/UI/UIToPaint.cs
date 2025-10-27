using UnityEngine;
using UnityEngine.UI;

public class UIToPaint : MonoBehaviour
{
    public UIToPaintType Type = UIToPaintType.Color1;
    [HideInInspector] public Image UiImage;

    private void Start()
    {
        UiImage = GetComponent<Image>();
        UIPainter.Instance.RegisterUIElement(this);
    }
    private void OnDestroy()
    {
        UIPainter.Instance.UnregisterUIElement(this);
    }
}