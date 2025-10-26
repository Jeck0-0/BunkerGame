using System.IO;
using UnityEngine;
using Networking;

public class STC_FactionInformation : BasePacket
{
    public override PacketType Type => PacketType.STC_FactionInformation;

    public uint playerId;
    public string name;
    public Color color;
    
    public STC_FactionInformation() { }
    public STC_FactionInformation(uint playerId, string name, Color color)
    {
        this.playerId = playerId;
        this.name = name;
        this.color = color;
    }
    
    public override void Serialize(BinaryWriter bw)
    {
        bw.Write((int)Type);
        bw.Write(playerId);
        bw.Write(name);
        bw.Write(color.r);
        bw.Write(color.g);
        bw.Write(color.b);
    }

    public override BasePacket Deserialize(BinaryReader br)
    {
        playerId = br.ReadUInt32();
        name = br.ReadString();
        color = new Color(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
        return this;
    }
}