using UnityEngine;

public abstract class ResourceManager<T> : Singleton<T> where T : ResourceManager<T>
{
    protected int influence;
    protected int materials;
    public static int Influence => instance.influence;
    public static int Materials => instance.materials;

    public static void ModifyInfluence(int amount)
    {
        instance.influence += amount;
        if (instance.influence < 0)
        {
            Debug.LogError($"Influence should not be negative ({instance.influence - amount} - {amount} = {instance.influence})");
            instance.influence = 0;
        }
    }
    public static void ModifyMaterials(int amount)
    {
        instance.materials += amount;
        if (instance.materials < 0)
        {
            Debug.LogError($"Materials should not be negative ({instance.materials - amount} - {amount} = {instance.materials})");
            instance.materials = 0;
        }
    }
}