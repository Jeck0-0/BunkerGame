using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct FactionData
{
    public string FactionName;

    public Color PlayerColor;

    // Emblem data
    public int BackgroundId;
    public Color[] BackgroundColors;

    public int SymbolId;
    public Color SymbolColor;
    public Vector2 SymbolPosition;
    public float SymbolScale;
    public float SymbolRotation;
}