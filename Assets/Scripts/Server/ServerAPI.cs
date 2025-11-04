using Client;
using Networking;

public static class ServerAPI
{
    public static void StartCrisis(Emergency emergency)
    {
        STC_StartEmergency packet = new STC_StartEmergency();
        NetworkManager.Server.SendToAll(packet);
    }

    public static void SendCrisisResult()
    {
        
    }

    public static void SendPlayerInfo()
    {
        
    }
}