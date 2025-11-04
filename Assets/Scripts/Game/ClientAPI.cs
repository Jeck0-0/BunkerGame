using Networking;
using Packets;
using UnityEngine;

public static class ClientAPI
{
    public static void SendFactionInfo()
    {
        
    }
    
    public static void ContributeToCrisis(TrackAmount tracks)
    {
        CTS_ContributeToCrisis packet = new CTS_ContributeToCrisis(tracks);
        NetworkManager.Client.Send(packet);
    }

    public static void VoteDilemma()
    {
        
    }
}