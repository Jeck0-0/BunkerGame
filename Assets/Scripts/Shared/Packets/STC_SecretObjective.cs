using Networking;
using System.IO;

namespace Packets
{
    public class STC_SecretObjective : BasePacket
    {
        public override PacketType Type => PacketType.STC_SecretObjective;
        public string ObjectiveId;

        public STC_SecretObjective() { }
        public STC_SecretObjective(SecretObjective objective)
        {
            this.ObjectiveId = objective.name;
        }

        public override void Serialize(BinaryWriter bw)
        {
            bw.Write((int)Type);
            bw.Write(ObjectiveId);
        }

        public override BasePacket Deserialize(BinaryReader br)
        {
            ObjectiveId = br.ReadString();
            return this;
        }
    }
}