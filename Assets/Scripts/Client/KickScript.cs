using Client;
using Server;
using Packets;
using Networking;
using UnityEngine;
using System.Linq;

public class KickScript : MonoBehaviour
{
    public int targetSpot;

    public void Kick()
    {
        var KickId = ClientPlayers.Instance.GetAll().FirstOrDefault(x => x.spot == targetSpot).id;
        //NetworkManager.Server.KickPlayer(KickId);
        Debug.Log($"Kick request sent for spot {KickId}");
    }
}
