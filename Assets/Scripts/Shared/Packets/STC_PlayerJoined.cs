using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Networking;

namespace Packets
{
    public class STC_PlayerJoined : BasePacket
    {
        public override PacketType Type => PacketType.STC_PlayerJoined;

        public uint playerId;
        public string username;
        public EmblemData emblemData;

        public STC_PlayerJoined() { }
        public STC_PlayerJoined(uint playerId, string username, EmblemData emblemData)
        {
            this.playerId = playerId;
            this.username = username;
            this.emblemData = emblemData;
        }

        public override void Serialize(BinaryWriter bw)
        {
            bw.Write((int)Type);
            bw.Write(playerId);
            bw.Write(username);
            PacketUtils.SerializeFactionData(bw, emblemData);
        }

        public override BasePacket Deserialize(BinaryReader br)
        {
            playerId = br.ReadUInt32();
            username = br.ReadString();
            emblemData = PacketUtils.DeserializeFactionData(br);
            return this;
        }
    }
}