using System.IO;
using Client;
using Networking;
using UnityEngine;

namespace Packets
{
    public class STC_StartEmergency : BasePacket
    {
        public override PacketType Type => PacketType.STC_StartEmergency;

        public EmergencyType emergencyType;
        public string crisisId;
        public double startTime;
        public float duration;

        public STC_StartEmergency() { }
        public STC_StartEmergency(Emergency emergency, double startTime)
        {
            emergencyType = emergency.Type;
            crisisId = emergency.name;
            duration = emergency.TimeToResolve;
            this.startTime = startTime;
        }

        public override void Serialize(BinaryWriter bw)
        {
            bw.Write((int)Type);
            bw.Write((int)emergencyType);
            bw.Write(crisisId);
            bw.Write(startTime);
            bw.Write(duration);
        }

        public override BasePacket Deserialize(BinaryReader br)
        {
            emergencyType = (EmergencyType)br.ReadInt32();
            crisisId = br.ReadString();
            startTime = br.ReadDouble();
            duration = br.ReadSingle();
            return this;
        }
    }
}