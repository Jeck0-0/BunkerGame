using System.IO;
using UnityEngine;
using Networking;

public class STC_CrisisResult : BasePacket
{
    public override PacketType Type => PacketType.STC_CrisisResult;

    public bool success;
    public TrackAmount TrackReward; // subtract if crisis failed
    //public bool highestBidder;
    //public bool lowestBidder;
    
    public STC_CrisisResult() { }
    public STC_CrisisResult(bool success, TrackAmount trackReward)
    {
        this.success = success;
        this.TrackReward = trackReward;
    }
    
    public override void Serialize(BinaryWriter bw)
    {
        bw.Write((int)Type);
        bw.Write(success);
        PacketUtils.SerializeTrackAmount(bw, TrackReward);
    }

    public override BasePacket Deserialize(BinaryReader br)
    {
        success = br.ReadBoolean();
        TrackReward = PacketUtils.DeserializeTrackAmount(br);
        return this;
    }
}