using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EmblemData
{
    public string FactionName = "";
    public Color PlayerColor;
    public Vector2 originalEmblemSize = new Vector2(400, 400); // for scalling
    public string PatternID;
    public List<Color> LayerColors = new List<Color>();

    public List<SymbolData> Symbols = new List<SymbolData>();

    [System.Serializable]
    public class SymbolData
    {
        public string SymbolID;
        public Color Color;
        public Vector3 Position;
        public float Scale = 1f;
        public float Rotation;
    }
}