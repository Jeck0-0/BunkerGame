using System.IO;

public static class PacketUtils
{
    public static void SerializeResourceAmount(BinaryWriter br, ResourceAmount amount)
    {
        br.Write(amount.Amount.Count);
        foreach (var a in amount.Amount)
        {
            br.Write((int)a.Key);
            br.Write(a.Value);
        }
    }

    public static ResourceAmount DeserializeResourceAmount(BinaryReader br)
    {
        ResourceAmount amount = new ResourceAmount();
        int count = br.ReadInt32();
        for (int i = 0; i < count; i++)
        {
            int type = br.ReadInt32();
            int quantity = br.ReadInt32();
            amount.Add((ResourceType)type, quantity);
        }
        return amount;
    }
    
}