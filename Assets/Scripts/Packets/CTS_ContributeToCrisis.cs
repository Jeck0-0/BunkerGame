using System.IO;
using Networking;

namespace Packets
{
    public class CTS_ContributeToCrisis : BasePacket
    {
        public override PacketType Type => PacketType.CTS_ContributeToCrisis;

        public TrackAmount TrackAmount;

        public CTS_ContributeToCrisis() { }
        public CTS_ContributeToCrisis(TrackAmount trackAmount)
        {
            this.TrackAmount = trackAmount;
        }
    
        public override void Serialize(BinaryWriter bw)
        {
            PacketUtils.SerializeTrackAmount(bw, TrackAmount);
            bw.Write((int)Type);
        }

        public override BasePacket Deserialize(BinaryReader br)
        {
            TrackAmount = PacketUtils.DeserializeTrackAmount(br);
            return this;
        }
    }
}