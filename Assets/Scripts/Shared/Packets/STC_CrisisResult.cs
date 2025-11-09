using System.IO;
using UnityEngine;
using Networking;

namespace Packets
{
    public class STC_CrisisResult : BasePacket
    {
        public override PacketType Type => PacketType.STC_CrisisResult;

        public bool success;
        public int materialsMod;
        public TrackAmount TrackMod; // subtract if crisis failed
                                     //public bool highestBidder;
                                     //public bool lowestBidder;

        public STC_CrisisResult() { }
        public STC_CrisisResult(bool success, int materialsMod, TrackAmount trackMod)
        {
            this.success = success;
            this.materialsMod = materialsMod;
            this.TrackMod = trackMod;
        }

        public override void Serialize(BinaryWriter bw)
        {
            bw.Write((int)Type);
            bw.Write(success);
            bw.Write(materialsMod);
            PacketUtils.SerializeTrackAmount(bw, TrackMod);
        }

        public override BasePacket Deserialize(BinaryReader br)
        {
            success = br.ReadBoolean();
            materialsMod = br.ReadInt32();
            TrackMod = PacketUtils.DeserializeTrackAmount(br);
            return this;
        }
    }
}