using System.IO;
using Networking;

namespace Packets
{
    public class STC_JoinResponse : BasePacket
    {
        public override PacketType Type => PacketType.STC_JoinResponse;

        //public uint playerId;
        public int spot;

        public STC_JoinResponse() { }
        public STC_JoinResponse(int spot)
        {
            //this.playerId = playerId;
            this.spot = spot;
        }

        public override void Serialize(BinaryWriter bw)
        {
            bw.Write((int)Type);
            //bw.Write(playerId);
            bw.Write(spot);
        }

        public override BasePacket Deserialize(BinaryReader br)
        {
            //playerId = br.ReadUInt32();
            spot = br.ReadInt32();
            return this;
        }
    }
}