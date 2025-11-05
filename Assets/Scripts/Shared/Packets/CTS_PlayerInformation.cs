using System.IO;
using UnityEngine;
using Networking;

public class CTS_PlayerInformation : BasePacket
{
    public override PacketType Type => PacketType.CTS_FactionInformation;

    public string username;
    public EmblemData emblemData;

    public CTS_PlayerInformation() { }
    public CTS_PlayerInformation(string username, EmblemData emblemData)
    {
        this.username = username;
        this.emblemData = emblemData;
    }
    
    public override void Serialize(BinaryWriter bw)
    {
        bw.Write((int)Type);
        bw.Write(username);
        PacketUtils.SerializeFactionData(bw, emblemData);
    }

    public override BasePacket Deserialize(BinaryReader br)
    {
        username = br.ReadString();
        emblemData = PacketUtils.DeserializeFactionData(br);
        return this;
    }
}