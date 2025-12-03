using Networking;
using Packets;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Packets
{
    public class STC_GameResault : BasePacket
    {
        public override PacketType Type => PacketType.STC_GameResault;

        public List<PlayerResult> _results = new List<PlayerResult>();
        public STC_GameResault() { }
        public STC_GameResault(List<PlayerResult> _results)
        {
            this._results = _results;
        }

        public override void Serialize(BinaryWriter bw)
        {
            bw.Write((int)Type);
            bw.Write(_results.Count);
            foreach (PlayerResult result in _results)
            {
                bw.Write(result.Player);
                bw.Write(result.VP);
            }
        }

        public override BasePacket Deserialize(BinaryReader br)
        {
            int ResaultCount = br.ReadInt32();

            for (int i = 0; i < ResaultCount; i++)
            {
                _results.Add(new PlayerResult { Player = (uint)br.ReadInt32(), VP = br.ReadInt32()});
            }
            return this;
        }
    }
    public struct PlayerResult
    {
        public uint Player;
        public int VP;
    }
}