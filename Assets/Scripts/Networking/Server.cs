using System;
using System.Linq;

public abstract class Server : ScriptableVariable<Client>
{
    public abstract void Connect(int maxPlayers);
    public abstract void SendTo(uint user, BasePacket packet);
    public abstract void SendToAll(BasePacket packet);
    public abstract void SendToAllExcept(uint user, BasePacket packet);
    public abstract void Subscribe<T>(Action<uint, BasePacket> callback) where T : BasePacket;
    public abstract void SubscribeToAll(Action<uint, BasePacket> callback);
    public abstract void Unsubscribe<T>(Action<uint, BasePacket> callback) where T : BasePacket;
    public abstract void UnsubscribeFromAll(Action<uint, BasePacket> callback);
    public abstract void Disconnect();
    public abstract void Update();
}