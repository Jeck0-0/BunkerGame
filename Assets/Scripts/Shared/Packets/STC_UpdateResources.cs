using Networking;
using System.IO;

namespace Packets
{
    public class STC_UpdateResources : BasePacket
    {
        public override PacketType Type => PacketType.STC_UpdateResources;

        public int materials;
        public int influence;

        public STC_UpdateResources() { }

        public STC_UpdateResources(int materials, int influence)
        {
            this.materials = materials;
            this.influence = influence;
        }

        public override void Serialize(BinaryWriter bw)
        {
            bw.Write((int)Type);
            bw.Write(materials);
            bw.Write(influence);
        }

        public override BasePacket Deserialize(BinaryReader br)
        {
            materials = br.ReadInt32();
            influence = br.ReadInt32();
            return this;
        }
    }
}
