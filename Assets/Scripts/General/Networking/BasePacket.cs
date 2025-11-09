using System.IO;
using Packets;
using UnityEngine;

namespace Networking
{
    public enum PacketType
    {
        None,
        CTS_FactionInformation = 0,
        CTS_ContributeToCrisis = 1,
        CTS_VoteOnDilemma = 2,


        STC_PlayerJoined = 100,
        STC_StartEconomyPhase = 101,
        STC_StartEmergency = 102,
        STC_CrisisResult = 103,
        STC_DilemmaResult = 104
    }

    public abstract class BasePacket
    {
        public abstract PacketType Type { get; }

        public abstract void Serialize(BinaryWriter bw);
        public abstract BasePacket Deserialize(BinaryReader br);

        public static BasePacket DeserializePacket(BinaryReader br)
        {
            var type = (PacketType)br.ReadInt32();

            switch (type)
            {
                case PacketType.CTS_FactionInformation: return new CTS_PlayerInformation().Deserialize(br);
                case PacketType.CTS_VoteOnDilemma: return new CTS_VoteOnDilemma().Deserialize(br);
                case PacketType.CTS_ContributeToCrisis: return new CTS_ContributeToCrisis().Deserialize(br);

                case PacketType.STC_PlayerJoined: return new STC_PlayerJoined().Deserialize(br);
                case PacketType.STC_StartEmergency:   return new STC_StartEmergency().Deserialize(br);
                case PacketType.STC_CrisisResult: return new STC_CrisisResult().Deserialize(br);
                case PacketType.STC_DilemmaResult: return new STC_DilemmaResult().Deserialize(br);

            }

            Debug.Log("Unknown packet type: " + type);
            return null;
        }
    }
}