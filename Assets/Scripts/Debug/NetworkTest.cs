using System;
using UnityEngine;
using Networking;

public class NetworkTest : MonoBehaviour
{
    private void Start()
    {
        NetworkManager.Client.Subscribe<STC_PlayerJoined>(ReceiveFactionInfo);
        NetworkManager.Server.Subscribe<CTS_FactionInformation>(ServerSendsInfoBack);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            //CLIENT sends test packet
            var factionInfoPacket = new CTS_FactionInformation("Test", new EmblemData {FactionName = "test"});
            NetworkManager.Client.Send(factionInfoPacket);
        }
    }
    
    private void ServerSendsInfoBack(uint playerId, BasePacket p)
    {
        //SERVER sends packet back
        CTS_FactionInformation packet = p as CTS_FactionInformation;
        
        var responsePacket = new STC_PlayerJoined(playerId, packet.username, packet.emblemData);
        NetworkManager.Server.SendToAllExcept(playerId, responsePacket);
    }

    private void ReceiveFactionInfo(BasePacket p)
    {
        //CLIENT prints received packet
        STC_PlayerJoined packet = p as STC_PlayerJoined;
        Debug.Log($"## [Client] Received Faction Info > [{packet.playerId}] {packet.username} #{packet.emblemData.FactionName}");
    }
}