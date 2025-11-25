using Networking;
using Packets;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;

namespace Server
{
    public class ServerTracks : Singleton<ServerTracks>
    {
        public int startValue = 5;
        public int maxValue = 10;

        [ShowInInspector, ReadOnly] protected Dictionary<TrackType, int> values = new();

        public event Action<TrackType> ResourceReachedZero;

        protected override void Awake()
        {
            base.Awake();
            values.Add(TrackType.Energy, startValue);
            values.Add(TrackType.Food, startValue);
            values.Add(TrackType.Moral, startValue);
            values.Add(TrackType.Order, startValue);
            values.Add(TrackType.Population, startValue);
        }


        public void ApplyModifier(TrackAmount amount)
        {
            foreach (var mod in amount.GetAll())
                ModifyResource(mod.Key, mod.Value);

            // STC_UpdateTracks packet = new STC_UpdateTracks();
            // NetworkManager.Server.SendToAll(packet);
        }

        protected void ModifyResource(TrackType type, int amount)
        {
            values[type] += amount;
            if (values[type] >= maxValue) values[type] = maxValue;
            if (values[type] <= 0) ResourceReachedZero?.Invoke(type);
        }

        public bool GetTrackValue(TrackType type, out int value)
        {
            return values.TryGetValue(type, out value);
        }
    }
}