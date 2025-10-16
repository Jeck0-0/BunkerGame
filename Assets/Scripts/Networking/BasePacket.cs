using System.IO;
using UnityEngine;

public enum PacketType
{
    None, 
    CTS_FactionInformation, 
    STC_FactionInformation, 
    STC_ProduceResource
}

public abstract class BasePacket
{
    public abstract PacketType Type { get; protected set; }

    public abstract void Serialize(BinaryWriter bw);
    protected abstract BasePacket Deserialize(BinaryReader br);
    
    public static BasePacket DeserializePacket(BinaryReader br)
    {
        var type = (PacketType)br.ReadInt32();

        switch (type)
        {
            case PacketType.CTS_FactionInformation:
                //decompile test1
                return null;
            case PacketType.STC_FactionInformation:
                //decompile test2
                return null;
        }
        Debug.Log("Unknown packet type: " + type);
        return null;
    }
    
    
}