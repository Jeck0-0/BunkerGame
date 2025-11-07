using UnityEngine;

public class PlayerResources : IPlayerResources
{
    protected int influence;
    protected int materials;
    public int Influence => influence;
    public int Materials => materials;

    public void ModifyInfluence(int amount)
    {
        influence += amount;
        if (influence < 0)
        {
            Debug.LogError($"Influence should not be negative ({influence - amount} - {amount} = {influence})");
            influence = 0;
        }
    }
    public void ModifyMaterials(int amount)
    {
        materials += amount;
        if (materials < 0)
        {
            Debug.LogError($"Materials should not be negative ({materials - amount} - {amount} = {materials})");
            materials = 0;
        }
    }
}

public interface IPlayerResources
{
    public int Influence { get; }
    public int Materials { get; }

    public void ModifyInfluence(int amount);
    public void ModifyMaterials(int amount);
}