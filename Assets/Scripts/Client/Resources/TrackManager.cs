using System;
using System.Collections.Generic;
using Sirenix.Utilities;

namespace Client
{
    public class TrackManager : Singleton<TrackManager>
    {
        public int startValue = 5;
        public int maxValue = 10;
        
        protected Dictionary<TrackType, int> values = new ();

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
            foreach (var mod in amount.Values)
                ModifyResource(mod.Key, mod.Value);
        }
        
        public void ModifyResource(TrackType type, int amount)
        {
            values[type] += amount;
            if (values[type] >= maxValue) values[type] = maxValue;
            if (values[type] <= 0) ResourceReachedZero?.Invoke(type);
        }
        
        
    }
}