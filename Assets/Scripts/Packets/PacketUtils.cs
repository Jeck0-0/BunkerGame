using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class PacketUtils
{
    public static void SerializeTrackAmount(BinaryWriter bw, TrackAmount amount)
    {
        if (amount == null)
        {
            bw.Write(0);
            return;
        }
        bw.Write(amount.Values.Count);
        foreach (var a in amount.Values)
        {
            bw.Write((int)a.Key);
            bw.Write(a.Value);
        }
    }
    public static TrackAmount DeserializeTrackAmount(BinaryReader br)
    {
        TrackAmount amount = new TrackAmount();
        int count = br.ReadInt32();
        for (int i = 0; i < count; i++)
        {
            int type = br.ReadInt32();
            int quantity = br.ReadInt32();
            amount.Add((TrackType)type, quantity);
        }
        return amount;
    }

    public static void SerializeRGBColor(BinaryWriter bw, Color color)
    {
        bw.Write(color.r);
        bw.Write(color.g);
        bw.Write(color.b);
    }
    public static Color DeserializeRGBColor(BinaryReader br)
    {
        return new Color(
            br.ReadSingle(), 
            br.ReadSingle(), 
            br.ReadSingle());
    }
    
    public static void SerializeRGBAColor(BinaryWriter bw, Color color)
    {
        SerializeRGBColor(bw, color);
        bw.Write(color.a);
    }
    public static Color DeserializeRGBAColor(BinaryReader br)
    {
        var color = DeserializeRGBColor(br);
        color.a = br.ReadSingle();
        return color;
    }
    
    public static void SerializeVector3(BinaryWriter bw, Vector3 vector)
    {
        bw.Write(vector.x);
        bw.Write(vector.y);
        bw.Write(vector.z);
    }
    public static Vector3 DeserializeVector3(BinaryReader br)
    {
        return new Vector3(
            br.ReadSingle(), 
            br.ReadSingle(), 
            br.ReadSingle());
    }

    public static void SerializeFactionData(BinaryWriter bw, EmblemData data)
    {
        bw.Write(data.FactionName);
        SerializeRGBColor(bw, data.PlayerColor);
        bw.Write(data.PatternID);
        
        bw.Write(data.LayerColors.Count);
        foreach (var bgColor in data.LayerColors)
            SerializeRGBColor(bw, bgColor);
        
        bw.Write(data.Symbols.Count);
        foreach (var symbol in data.Symbols)
        {
            bw.Write(symbol.SymbolID);
            SerializeRGBColor(bw, symbol.Color);
            SerializeVector3(bw, symbol.Position);
            bw.Write(symbol.Scale);
            bw.Write(symbol.Rotation);
        }
    }
    public static EmblemData DeserializeFactionData(BinaryReader br)
    {
        var data = new  EmblemData();
        data.FactionName = br.ReadString();
        data.PlayerColor = DeserializeRGBColor(br);
        data.PatternID = br.ReadString();

        data.LayerColors = new List<Color>();
        int layerCount = br.ReadInt32();
        for (int i = 0; i < layerCount; i++)
            data.LayerColors.Add(DeserializeRGBColor(br));

        data.Symbols = new List<EmblemData.SymbolData>();
        int symbolCount = br.ReadInt32();
        for (int i = 0; i < symbolCount; i++)
        {
            data.Symbols.Add(new EmblemData.SymbolData());
            data.Symbols[i].SymbolID = br.ReadString();
            data.Symbols[i].Color = DeserializeRGBColor(br);
            data.Symbols[i].Position = DeserializeVector3(br);
            data.Symbols[i].Scale = br.ReadSingle();
            data.Symbols[i].Rotation = br.ReadSingle();
        }
        return data;
    }
}