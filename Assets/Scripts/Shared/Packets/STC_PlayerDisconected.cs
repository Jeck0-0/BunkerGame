using Networking;
using System.IO;
using UnityEngine;
namespace Packets
{
    public class STC_PlayerDisconected : BasePacket
    {
        public override PacketType Type => PacketType.STC_PlayerDisconected;
        public int spot;

        public STC_PlayerDisconected() { }
        public STC_PlayerDisconected(int spot)
        {
            this.spot = spot;
        }
        public override void Serialize(BinaryWriter bw)
        {
            bw.Write((int)Type);
            bw.Write(spot);
        }
        public override BasePacket Deserialize(BinaryReader br)
        {
            spot = br.ReadInt32();
            return this;
        }
    }
}
