using System;

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
public struct ResourceAmount
{
    public ResourceType Type;
    public int Amount;
    public ResourceAmount(ResourceType t, int a) { Type = t; Amount = a; }
}