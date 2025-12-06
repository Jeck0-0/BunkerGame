using Networking;
using System.IO;

namespace Packets
{
    public class STC_PlayerVoted : BasePacket
    {
        public override PacketType Type => PacketType.STC_PlayerVoted;

        public uint PlayerId;
        public int OptionIndex;

        public STC_PlayerVoted() { }
        public STC_PlayerVoted(uint playerId, int optionIndex)
        {
            PlayerId = playerId;
            OptionIndex = optionIndex;
        }

        public override void Serialize(BinaryWriter bw)
        {
            bw.Write((int)Type);
            bw.Write(PlayerId);
            bw.Write(OptionIndex);
        }

        public override BasePacket Deserialize(BinaryReader br)
        {
            PlayerId = br.ReadUInt32();
            OptionIndex = br.ReadInt32();
            return this;
        }
    }
}