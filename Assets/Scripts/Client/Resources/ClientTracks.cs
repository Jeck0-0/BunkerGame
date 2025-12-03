using Networking;
using Packets;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;

namespace Client
{
    public class ClientTracks : Singleton<ClientTracks>
    {
        public int startValue = 5;
        public int maxValue = 10;
        
        [ShowInInspector, ReadOnly] protected Dictionary<TrackType, int> values = new ();

        public event Action<TrackType> ResourceReachedZero;
        
        protected override void Awake()
        {
            values.Add(TrackType.Energy, startValue);
            values.Add(TrackType.Food, startValue);
            values.Add(TrackType.Moral, startValue);
            values.Add(TrackType.Order, startValue);
            values.Add(TrackType.Population, startValue);
            base.Awake();

            GameClient.Subscribe<STC_UpdateTracks>(ReceivePacket);
        }

        private void OnDestroy()
        {
            GameClient.Unsubscribe<STC_UpdateTracks>(ReceivePacket);
        }

        private void ReceivePacket(BasePacket p)
        {
            var packet = (STC_UpdateTracks)p;
            ApplyModifier(packet.Change);
        }

        public void ApplyModifier(TrackAmount amount)
        {
            foreach (var mod in amount.GetAll())
                ModifyResource(mod.Key, mod.Value);

            TrackUI.Instance.UpdateAllTracks();
        }
        
        public void ModifyResource(TrackType type, int amount)
        {
            values[type] += amount;
            if (values[type] >= maxValue) values[type] = maxValue;
            if (values[type] <= 0) ResourceReachedZero?.Invoke(type);
        }

        public int GetTrackValue(TrackType type)
        {
            if (values.TryGetValue(type, out int val))
                return val;
            return 0;
        }
    }
}