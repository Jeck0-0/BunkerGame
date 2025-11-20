using System.IO;
using Networking;

namespace Packets
{
    public class STC_PlayerJoined : BasePacket
    {
        public override PacketType Type => PacketType.STC_PlayerJoined;

        public uint playerId;
        public string username;
        public int spot;
        public EmblemData emblemData;

        public STC_PlayerJoined() { }
        public STC_PlayerJoined(uint playerId, string username, int spot, EmblemData emblemData)
        {
            this.playerId = playerId;
            this.username = username ?? string.Empty;
            this.spot = spot;
            this.emblemData = emblemData;
        }

        public override void Serialize(BinaryWriter bw)
        {
            bw.Write((int)Type);
            bw.Write(playerId);
            bw.Write(username);
            bw.Write(spot);
            PacketUtils.SerializeFactionData(bw, emblemData);
        }

        public override BasePacket Deserialize(BinaryReader br)
        {
            playerId = br.ReadUInt32();
            username = br.ReadString();
            spot = br.ReadInt32();
            emblemData = PacketUtils.DeserializeFactionData(br);
            return this;
        }
    }
}