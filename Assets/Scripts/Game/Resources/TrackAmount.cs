using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[Serializable]
public class TrackAmount
{
    public Dictionary<TrackType, int> Values;
    public TrackAmount() { }
    public TrackAmount(Dictionary<TrackType, int> values) { Values = values; }
    public TrackAmount(TrackType type, int amount) { Values = new Dictionary<TrackType, int> {{type, amount}}; }

    
    public bool Has(TrackType type) => Values.ContainsKey(type);
    public bool Has(IEnumerable<TrackType> type) => type.All(x => Values.ContainsKey(x)); 
    public bool Has(TrackType type, int amount) 
        => (Values.ContainsKey(type) || amount == 0) && Values[type] >= amount;
    public bool Has(TrackAmount amount)
        => amount.Values.All(x => Has(x.Key, x.Value));

    
    public TrackAmount Add(TrackType type, int amount)
    {
        Values.TryAdd(type, 0);
        Values[type] += amount;
        return this;
    }
    public TrackAmount Add(TrackAmount amount) 
    {
        foreach (var x in Values.Keys)
            Add(x, amount.Values[x]);
        return this;
    }

    public TrackAmount Subtract(TrackType type, int amount)
    {
        Values.TryAdd(type, 0);
        Values[type] -= amount;
        return this;
    }
    public TrackAmount Subtract(TrackAmount amount)
    {
        foreach (var x in Values.Keys)
            Subtract(x, amount.Values[x]);
        return this;
    }
    
    public static TrackAmount operator - (TrackAmount a, TrackAmount b) => a.Subtract(b);
    public static TrackAmount operator + (TrackAmount a, TrackAmount b) => a.Add(b);
    
}