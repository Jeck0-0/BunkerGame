using System.IO;
using Networking;

namespace Packets
{
    public class STC_UpdateTracks : BasePacket
    {
        public override PacketType Type => PacketType.STC_UpdateTracks;

        public TrackAmount Change;

        public STC_UpdateTracks() { }
        public STC_UpdateTracks(TrackAmount change)
        {
            this.Change = change;
        }

        public override void Serialize(BinaryWriter bw)
        {
            bw.Write((int)Type);
            PacketUtils.SerializeTrackAmount(bw, Change);
        }

        public override BasePacket Deserialize(BinaryReader br)
        {
            Change = PacketUtils.DeserializeTrackAmount(br);
            return this;
        }
    }
}