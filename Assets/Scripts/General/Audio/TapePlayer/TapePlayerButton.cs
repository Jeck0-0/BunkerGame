using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TapePlayerButton : MonoBehaviour
{
    [SerializeField] TapePlayer tapePlayer;

    [Header("Button Type")]
    [SerializeField] TapeButtonAction action;

    [Header("Visuals")]
    [SerializeField] Renderer buttonRenderer;
    [SerializeField] Color hoverColor = Color.yellow;
    private Color defaultColor;

    private Material mat;
    private Color currentColor;

    public enum TapeButtonAction
    {
        PlayPause,
        Stop,
        Next,
        Loop,
        Eject
    }

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

    public void OnPressed()
    {
        if (tapePlayer == null)
        {
            Debug.LogWarning($"{name} has no TapePlayer assigned.");
            return;
        }

        switch (action)
        {
            case TapeButtonAction.PlayPause:
                tapePlayer.PlayOrPause();
                break;

            case TapeButtonAction.Stop:
                tapePlayer.Stop();
                break;

            case TapeButtonAction.Next:
                tapePlayer.NextTrack();
                break;

            case TapeButtonAction.Loop:
                tapePlayer.ToggleLoop();
                break;

            case TapeButtonAction.Eject:
                tapePlayer.EjectTape();
                break;

            default:
                Debug.LogWarning($"Unhandled button action: {action}");
                break;
        }
    }
}