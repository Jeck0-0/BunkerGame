using UnityEngine;

[CreateAssetMenu(fileName = "Painter Settings", menuName = "Scriptable Objects/Painter Settings")]
public class PainterSettings : ScriptableObject
{
    [Header("Basic Colors")]
    public Color Color1 = Color.white;
    public Color Color2 = Color.grey;
    public Color Color3 = Color.black;

    [Header("Flicker")]
    public Color FlickerStartColor = Color.white;
    public Color FlickerEndColor = Color.whiteSmoke;
    public float FlickerSpeed = 2f;
}