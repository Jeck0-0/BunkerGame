using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum ResourceType : byte
{
    Food = 0,
    Materials = 1,
    Medicine = 2,
    Weapons = 3,
    Tools = 4,
    Sabotage_Kitts = 5
}


[Serializable]
public class ResourceAmount
{
    public Dictionary<ResourceType, int> Amount;
    public ResourceAmount(Dictionary<ResourceType, int> amount) { Amount = amount; }
    public ResourceAmount(ResourceType type, int amount) { Amount = new Dictionary<ResourceType, int> {{type, amount}}; }

    
    public bool Has(ResourceType type, int amount) 
        => (Amount.ContainsKey(type) || amount == 0) && Amount[type] >= amount;
    public bool Has(ResourceAmount amount)
        => amount.Amount.All(x => Has(x.Key, x.Value));

    public void Add(ResourceType type, int amount)
    {
        Amount.TryAdd(type, 0);
        Amount[type] += amount;
    }
    public void Add(ResourceAmount amount)
    {
        foreach (var x in Amount.Keys)
            Add(x, amount.Amount[x]);
    }

    public void Subtract(ResourceType type, int amount)
    {
        Amount.TryAdd(type, 0);
        Amount[type] -= amount;
        Debug.LogWarning($"Resource below zero: {type.ToString()} {Amount[type]}");
    }
    public void Subtract(ResourceAmount amount)
    {
        foreach (var x in Amount.Keys)
            Subtract(x, amount.Amount[x]);
    }
    
}