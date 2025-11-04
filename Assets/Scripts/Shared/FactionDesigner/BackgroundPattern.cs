using UnityEngine;

[CreateAssetMenu(fileName = "BackgroundPattern", menuName = "FactionDesigner/BackgroundPattern")]
public class BackgroundPattern : ScriptableObject
{
    public Sprite[] Layers;
}