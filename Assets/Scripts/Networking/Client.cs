using System;

public abstract class Client : ScriptableVariable<Client>
{
    public abstract void Connect();
    public abstract void Send(BasePacket packet);
    public abstract void Subscribe<T>(Action<BasePacket> callback) where T : BasePacket;
    public abstract void Unsubscribe<T>(Action<BasePacket> callback) where T : BasePacket;
    public abstract void Disconnect();
    public abstract void Update();
}