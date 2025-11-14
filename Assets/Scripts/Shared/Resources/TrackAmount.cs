using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;
using UnityEngine;


[Serializable]
public class TrackAmount
{
    [SerializeField] protected int order;
    [SerializeField] protected int population;
    [SerializeField] protected int food;
    [SerializeField] protected int moral;
    [SerializeField] protected int energy;
    
    public TrackAmount() { }
    public TrackAmount(TrackType type, int amount) 
        => Modify(type, _ => amount); 
    public TrackAmount(Dictionary<TrackType, int> values)
        => values.ForEach(x => Modify(x.Key, _ => x.Value));

    
    public int Get(TrackType type)
    {
        switch (type)
        {
            case TrackType.Order: return order;
            case TrackType.Population: return population;
            case TrackType.Food: return food;
            case TrackType.Moral: return moral;
            case TrackType.Energy: return energy;
            default: return 0;
        }
    }
    public int Modify(TrackType type, Func<int, int> func)
    {
        switch (type)
        {
            case TrackType.Order: order = func.Invoke(order); return order;
            case TrackType.Population: population = func.Invoke(population); return population;
            case TrackType.Food: food = func.Invoke(food); return food;
            case TrackType.Moral: moral = func.Invoke(moral); return moral;
            case TrackType.Energy: energy = func.Invoke(energy); return energy;
            default: return 0;
        }
    }

    public Dictionary<TrackType, int> GetAll() => new() {
        { TrackType.Order, order },
        { TrackType.Population, population },
        { TrackType.Food, food },
        { TrackType.Moral, moral },
        { TrackType.Energy, energy }
    };
    
    public bool Has(TrackType type) => Get(type) > 0;
    public bool Has(IEnumerable<TrackType> type) => type.All(x => Get(x) > 0); 
    public bool Has(TrackType type, int amount) => Get(type) >= amount;

    
    public TrackAmount Add(TrackType type, int amount)
    {
        Modify(type, x => x + amount);
        return this;
    }
    public TrackAmount Add(TrackAmount amount) 
    {
        foreach (var x in amount.GetAll())
            Add(x.Key, x.Value);
        return this;
    }

    public TrackAmount Subtract(TrackType type, int amount)
    {
        Modify(type, x => x - amount);
        return this;
    }
    public TrackAmount Subtract(TrackAmount amount)
    {
        foreach (var x in amount.GetAll())
            Subtract(x.Key, x.Value);
        return this;
    }
    
    public static TrackAmount operator - (TrackAmount a, TrackAmount b) => a.Subtract(b);
    public static TrackAmount operator + (TrackAmount a, TrackAmount b) => a.Add(b);
    
}