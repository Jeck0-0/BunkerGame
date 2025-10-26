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
        
        STC_FactionInformation = 100,
        STC_StartEconomyPhase = 101,
        STC_StartCrisisPhase = 102,
        STC_CrisisResult = 103
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
                case PacketType.CTS_FactionInformation: return new CTS_FactionInformation().Deserialize(br);
                case PacketType.STC_FactionInformation: return new STC_FactionInformation().Deserialize(br);
                case PacketType.STC_StartCrisisPhase:   return new STC_StartCrisisPhase().Deserialize(br);
                case PacketType.CTS_ContributeToCrisis: return new CTS_ContributeToCrisis().Deserialize(br);
                    
            }

            Debug.Log("Unknown packet type: " + type);
            return null;
        }
    }
}