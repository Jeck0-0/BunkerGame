using System.IO;
using Networking;
using UnityEngine;

public class STC_StartCrisisPhase : BasePacket
{
    public override PacketType Type => PacketType.STC_StartCrisisPhase;

    public double startTime;
    public float duration;
    
    public STC_StartCrisisPhase() { }
    public STC_StartCrisisPhase(double startTime, float duration)
    {
        this.startTime = startTime;
        this.duration = duration;
    }
    
    public override void Serialize(BinaryWriter bw)
    {
        bw.Write((int)Type);
        bw.Write(startTime);
        bw.Write(duration);
    }

    public override BasePacket Deserialize(BinaryReader br)
    {
        startTime = br.ReadDouble();
        duration = br.ReadSingle();
        return this;
    }
}