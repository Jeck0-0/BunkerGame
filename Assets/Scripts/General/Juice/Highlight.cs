using TMPro;
using UnityEngine;

public class Highlight : MonoBehaviour
{
    public Color colorA = Color.red;
    public Color colorB = Color.blue;
    public float duration = 1f;

    private TextMeshProUGUI textUI;

    void Awake()
    {
        textUI = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        float t = Mathf.PingPong(Time.time / duration, 1f);
        textUI.color = Color.Lerp(colorA, colorB, t);
    }
}
