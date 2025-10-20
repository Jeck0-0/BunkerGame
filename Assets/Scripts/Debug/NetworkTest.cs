using System;
using UnityEngine;

public class NetworkTest : MonoBehaviour
{
    private void Start()
    {
        NetworkManager.Client.Subscribe<STC_FactionInformation>(ReceiveFactionInfo);
        NetworkManager.Server.Subscribe<CTS_FactionInformation>(ServerSendsInfoBack);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            //CLIENT sends test packet
            var factionInfoPacket = new CTS_FactionInformation("Test", Color.orange);
            NetworkManager.Client.Send(factionInfoPacket);
        }
    }
    
    private void ServerSendsInfoBack(uint playerId, BasePacket p)
    {
        //SERVER sends packet back
        CTS_FactionInformation packet = p as CTS_FactionInformation;
        
        var responsePacket = new STC_FactionInformation(playerId, packet.name, packet.color);
        NetworkManager.Server.SendToAllExcept(playerId, responsePacket);
    }

    private void ReceiveFactionInfo(BasePacket p)
    {
        //CLIENT prints received packet
        STC_FactionInformation packet = p as STC_FactionInformation;
        Debug.Log($"## [Client] Received Faction Info > [{packet.playerId}] {packet.name} #{packet.color}");
    }
}