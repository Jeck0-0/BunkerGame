using Networking;
using System.IO;

namespace Packets
{
    public class CTS_VoteOnDilemma : BasePacket
    {
        public override PacketType Type => PacketType.CTS_VoteOnDilemma;

        public int OptionIndex;
        public int InfluenceSpent;

        public CTS_VoteOnDilemma() { }
        public CTS_VoteOnDilemma(int optionIndex, int influenceSpent)
        {
            OptionIndex = optionIndex;
            InfluenceSpent = influenceSpent;
        }

        public override void Serialize(BinaryWriter bw)
        {
            bw.Write(OptionIndex);
            bw.Write(InfluenceSpent);
            bw.Write((int)Type);
        }

        public override BasePacket Deserialize(BinaryReader br)
        {
            OptionIndex = br.ReadInt32();
            InfluenceSpent = br.ReadInt32();
            return this;
        }
    }
}