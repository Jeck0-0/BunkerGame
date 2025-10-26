using System.IO;
using Networking;

namespace Packets
{
    public class CTS_ContributeToCrisis : BasePacket
    {
        public override PacketType Type => PacketType.CTS_ContributeToCrisis;

        public ResourceAmount resourceAmount;

        public CTS_ContributeToCrisis() { }
        public CTS_ContributeToCrisis(ResourceAmount resourceAmount)
        {
            this.resourceAmount = resourceAmount;
        }
    
        public override void Serialize(BinaryWriter bw)
        {
            PacketUtils.SerializeResourceAmount(bw, resourceAmount);
            bw.Write((int)Type);
        }

        public override BasePacket Deserialize(BinaryReader br)
        {
            resourceAmount = PacketUtils.DeserializeResourceAmount(br);
            return this;
        }
    }
}