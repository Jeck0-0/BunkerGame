using Networking;
using System.IO;

namespace Packets
{
    public class STC_GameStart : BasePacket
    {
        public override PacketType Type => PacketType.STC_GameStart;

        public STC_GameStart() { }

        public override void Serialize(BinaryWriter bw)
        {
            bw.Write((int)Type);
        }

        public override BasePacket Deserialize(BinaryReader br)
        {
            return this;
        }
    }
}