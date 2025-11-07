using System.IO;
using Networking;

namespace Packets
{
    public class CTS_ContributeToCrisis : BasePacket
    {
        public override PacketType Type => PacketType.CTS_ContributeToCrisis;

        public int materials;

        public CTS_ContributeToCrisis() { }
        public CTS_ContributeToCrisis(int materials)
        {
            this.materials = materials;
        }
    
        public override void Serialize(BinaryWriter bw)
        {
            bw.Write((int)Type);
            bw.Write(materials);
        }

        public override BasePacket Deserialize(BinaryReader br)
        {
            materials = br.ReadInt32();
            return this;
        }
    }
}