using System.IO;
using UnityEngine;

public class CTS_FactionInformation : BasePacket
{
    public override PacketType Type => PacketType.CTS_FactionInformation;

    public string name;
    public Color color;

    public CTS_FactionInformation() { }
    public CTS_FactionInformation(string name, Color color)
    {
        this.name = name;
        this.color = color;
    }
    
    public override void Serialize(BinaryWriter bw)
    {
        bw.Write((int)Type);
        bw.Write(name);
        bw.Write(color.r);
        bw.Write(color.g);
        bw.Write(color.b);
    }

    protected override BasePacket Deserialize(BinaryReader br)
    {
        name = br.ReadString();
        color = new Color(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
        return this;
    }
}