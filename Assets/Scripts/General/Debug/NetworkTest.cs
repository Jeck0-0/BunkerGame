using System;
using Packets;
using UnityEngine;
using Networking;

public class NetworkTest : MonoBehaviour
{
    private void Start()
    {
        NetworkManager.Server.Subscribe<STC_JoinResponse>(Respond);
        NetworkManager.Client.Subscribe<STC_JoinResponse>(ReceiveResponse);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            //CLIENT sends test packet
            var factionInfoPacket = new STC_JoinResponse(55);
            NetworkManager.Client.Send(factionInfoPacket);
        }
    }
    
    private void Respond(uint playerId, BasePacket p)
    {
        //SERVER sends packet back
        STC_JoinResponse packet = p as STC_JoinResponse;
        
        var responsePacket = new STC_JoinResponse(66);
        NetworkManager.Server.SendTo(playerId, responsePacket);
    }

    private void ReceiveResponse(BasePacket p)
    {
        //CLIENT prints received packet
        STC_JoinResponse packet = p as STC_JoinResponse;
        Debug.Log($"## [Client] > {packet.spot}");
    }
}