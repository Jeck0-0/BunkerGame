using UnityEngine;

[CreateAssetMenu(fileName = "SymbolType", menuName = "FactionDesigner/SymbolType")]
public class SymbolType : ScriptableObject
{
    public Sprite SymbolSprite;
    public bool Blank = false;
}
