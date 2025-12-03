using System.IO;
using System.Text;
using UnityEngine;

public static class EmblemSaveSystem
{
    private const string saveFolder = "Emblems";

    public static void Save(string fileName, EmblemData data)
    {
        string dir = Path.Combine(Application.persistentDataPath, saveFolder);
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        string path = Path.Combine(dir, fileName + ".txt");
        File.WriteAllText(path, Serialize(data));
    }

    public static EmblemData Load(string fileName)
    {
        string dir = Path.Combine(Application.persistentDataPath, saveFolder);
        string path = Path.Combine(dir, fileName + ".txt");

        if (!File.Exists(path)) return null;

        string[] lines = File.ReadAllLines(path);
        return Deserialize(lines);
    }

    private static string Serialize(EmblemData d)
    {
        StringBuilder sb = new StringBuilder();

        sb.AppendLine($"Name={d.FactionName}");
        sb.AppendLine($"PlayerColor={ToStr(d.PlayerColor)}");
        sb.AppendLine($"Pattern={d.PatternID}");

        sb.AppendLine($"LayerCount={d.LayerColors.Count}");
        for (int i = 0; i < d.LayerColors.Count; i++)
            sb.AppendLine($"LayerColor{i}={ToStr(d.LayerColors[i])}");

        sb.AppendLine($"SymbolCount={d.Symbols.Count}");
        for (int i = 0; i < d.Symbols.Count; i++)
        {
            var s = d.Symbols[i];
            sb.AppendLine($"Symbol{i}_ID={s.SymbolID}");
            sb.AppendLine($"Symbol{i}_Color={ToStr(s.Color)}");
            sb.AppendLine($"Symbol{i}_Pos={ToStr(s.Position)}");
            sb.AppendLine($"Symbol{i}_Scale={s.Scale}");
            sb.AppendLine($"Symbol{i}_Rot={s.Rotation}");
        }

        return sb.ToString();
    }

    private static EmblemData Deserialize(string[] lines)
    {
        EmblemData d = new EmblemData();
        int index = 0;

        string Get(string key)
        {
            while (index < lines.Length)
            {
                if (lines[index].StartsWith(key + "="))
                {
                    string value = lines[index].Substring(key.Length + 1);
                    index++;
                    return value;
                }
                index++;
            }
            return "";
        }

        index = 0;
        d.FactionName = Get("Name");
        d.PlayerColor = ParseColor(Get("PlayerColor"));
        d.PatternID = Get("Pattern");

        int layerCount = int.Parse(Get("LayerCount"));
        d.LayerColors.Clear();

        for (int i = 0; i < layerCount; i++)
            d.LayerColors.Add(ParseColor(Get($"LayerColor{i}")));

        int symbolCount = int.Parse(Get("SymbolCount"));
        d.Symbols.Clear();
        for (int i = 0; i < symbolCount; i++)
        {
            var s = new EmblemData.SymbolData();
            s.SymbolID = Get($"Symbol{i}_ID");
            s.Color = ParseColor(Get($"Symbol{i}_Color"));
            s.Position = ParseVector3(Get($"Symbol{i}_Pos"));
            s.Scale = float.Parse(Get($"Symbol{i}_Scale"));
            s.Rotation = float.Parse(Get($"Symbol{i}_Rot"));
            d.Symbols.Add(s);
        }

        return d;
    }

    private static string ToStr(Color c) => $"{c.r},{c.g},{c.b},{c.a}";

    private static string ToStr(Vector3 v) => $"{v.x},{v.y},{v.z}";

    private static Color ParseColor(string s)
    {
        var p = s.Split(',');
        return new Color(float.Parse(p[0]), float.Parse(p[1]), float.Parse(p[2]), float.Parse(p[3]));
    }

    private static Vector3 ParseVector3(string s)
    {
        var p = s.Split(',');
        return new Vector3(float.Parse(p[0]), float.Parse(p[1]), float.Parse(p[2]));
    }
}
