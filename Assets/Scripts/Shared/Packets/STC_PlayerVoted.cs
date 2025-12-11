using Networking;
using System.IO;

namespace Packets
{
    public class STC_PlayerVoted : BasePacket
    {
        public override PacketType Type => PacketType.STC_PlayerVoted;

        public uint PlayerId;
        public int OptionIndex;
        public int Influence;

        public STC_PlayerVoted() { }
        public STC_PlayerVoted(uint playerId, int optionIndex, int influence)
        {
            PlayerId = playerId;
            OptionIndex = optionIndex;
            Influence = influence;
        }

        public override void Serialize(BinaryWriter bw)
        {
            bw.Write((int)Type);
            bw.Write(PlayerId);
            bw.Write(OptionIndex);
            bw.Write(Influence);
        }

        public override BasePacket Deserialize(BinaryReader br)
        {
            PlayerId = br.ReadUInt32();
            OptionIndex = br.ReadInt32();
            Influence = br.ReadInt32();
            return this;
        }
    }
}