using TMPro;
using UnityEngine;

public class YourNamePlate : MonoBehaviour
{
    [SerializeField] TextMeshPro text;
    [SerializeField] Renderer screenRenderer;
    [SerializeField] Color hoverColor = Color.yellow;
    private Color defaultColor;
    private Material mat;

    private void Awake()
    {
        mat = screenRenderer.material;
        defaultColor = mat.color;
    }

    public void SetObjective(string role)
    {
        text.text = role;
    }

    public void ShowObjective()
    {
        SecretObjectivelUI.Instance.ShowObjective();
    }

    public void OnHoverEnter()
    {
        mat.color = hoverColor;
    }

    public void OnHoverExit()
    {
        mat.color = defaultColor;
    }
}
