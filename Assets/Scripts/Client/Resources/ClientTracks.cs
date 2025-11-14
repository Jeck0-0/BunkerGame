using System.Collections.Generic;
using System;
using Sirenix.OdinInspector;

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
        }


        public void ApplyModifier(TrackAmount amount)
        {
            foreach (var mod in amount.GetAll())
                ModifyResource(mod.Key, mod.Value);
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