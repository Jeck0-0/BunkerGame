using Networking;
using System.IO;

namespace Packets
{
    public class STC_DilemmaResult : BasePacket
    {
        public override PacketType Type => PacketType.STC_DilemmaResult;

        public int WinningOption;
        public TrackAmount TrackModifier;

        public STC_DilemmaResult() { }
        public STC_DilemmaResult(int winningOption, TrackAmount trackModifier)
        {
            this.WinningOption = winningOption;
            this.TrackModifier = trackModifier;
        }

        public override void Serialize(BinaryWriter bw)
        {
            bw.Write((int)Type);
            bw.Write(WinningOption);
            PacketUtils.SerializeTrackAmount(bw, TrackModifier ?? new TrackAmount());
        }

        public override BasePacket Deserialize(BinaryReader br)
        {
            WinningOption = br.ReadInt32();
            TrackModifier = PacketUtils.DeserializeTrackAmount(br);
            return this;
        }
    }
}
