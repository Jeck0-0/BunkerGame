using Networking;
using System.IO;
namespace Packets
{
    public class STC_PlayerDisconnected : BasePacket
    {
        public override PacketType Type => PacketType.STC_PlayerDisconected;

        public uint playerId;
        public int spot;

        public STC_PlayerDisconnected() { }
        public STC_PlayerDisconnected(uint playerId, int spot)
        {
            this.playerId = playerId;
            this.spot = spot;
        }
        public override void Serialize(BinaryWriter bw)
        {
            bw.Write((int)Type);
            bw.Write(playerId);
            bw.Write(spot);
        }
        public override BasePacket Deserialize(BinaryReader br)
        {
            playerId = br.ReadUInt32();
            spot = br.ReadInt32();
            return this;
        }
    }
}
