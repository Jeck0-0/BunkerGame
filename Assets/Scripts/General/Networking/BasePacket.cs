using System.IO;
using Packets;
using UnityEngine;

namespace Networking
{
    public enum PacketType
    {
        None = 0,
        CTS_PlayerInformation = 1,
        CTS_ContributeToCrisis = 2,
        CTS_VoteOnDilemma = 3,


        STC_PlayerJoined = 100,
        STC_JoinResponse = 101,
        STC_StartEmergency = 102,
        STC_CrisisResult = 103,
        STC_DilemmaResult = 104,
        STC_UpdateResources = 105,
        STC_UpdateTracks = 106,
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
                case PacketType.CTS_PlayerInformation: return new CTS_PlayerInformation().Deserialize(br);
                case PacketType.CTS_VoteOnDilemma: return new CTS_VoteOnDilemma().Deserialize(br);
                case PacketType.CTS_ContributeToCrisis: return new CTS_ContributeToCrisis().Deserialize(br);

                case PacketType.STC_PlayerJoined: return new STC_PlayerJoined().Deserialize(br);
                case PacketType.STC_JoinResponse: return new STC_JoinResponse().Deserialize(br);
                case PacketType.STC_StartEmergency:   return new STC_StartEmergency().Deserialize(br);
                case PacketType.STC_CrisisResult: return new STC_CrisisResult().Deserialize(br);
                case PacketType.STC_DilemmaResult: return new STC_DilemmaResult().Deserialize(br);
                case PacketType.STC_UpdateResources: return new STC_UpdateResources().Deserialize(br);
                case  PacketType.STC_UpdateTracks: return new STC_UpdateTracks().Deserialize(br);

                default: Debug.LogError("Unknown packet type: " + type);
                return null;

            }
        }
    }
}