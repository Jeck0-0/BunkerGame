using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TapeObject : MonoBehaviour
{
    [SerializeField] TapePlayer tapePlayer;
    [SerializeField] Tape tape;

    [Header("Visuals")]
    [SerializeField] Renderer buttonRenderer;
    [SerializeField] Color hoverColor = Color.yellow;
    private Color defaultColor;

    private Material mat;
    private Color _currentColor;

    private void Awake()
    {
        if (buttonRenderer == null)
        buttonRenderer = GetComponent<Renderer>();

        mat = buttonRenderer.material;
        defaultColor = mat.color;
    }

    public void OnHoverEnter()
    {
        mat.color = hoverColor;
    }

    public void OnHoverExit()
    {
        mat.color = defaultColor;
    }

    public void InsertTape()
    {
        tapePlayer.InsertTape(tape, this);
    }
}