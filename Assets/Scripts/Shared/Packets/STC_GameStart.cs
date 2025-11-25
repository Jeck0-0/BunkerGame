using Networking;
using System.IO;

namespace Packets
{
    public class STC_GameStart : BasePacket
    {
        public override PacketType Type => PacketType.STC_GameStart;
        public string ObjectiveId;

        public STC_GameStart() { }
        public STC_GameStart(SecretObjective objective)
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