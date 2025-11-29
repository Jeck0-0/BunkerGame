using System.Collections.Generic;
using Networking;
using Packets;
using TMPro;
using UnityEngine;

namespace Client
{
    public class ClientLobbyManager : MonoBehaviour
    {
        [SerializeField] GameObject playerEntryPrefab;
        [SerializeField] Transform playerEntryParent;
        private Dictionary<uint, GameObject> entries = new();
        private bool conected = false;

        void Awake()
        {
            GameClient.Subscribe<STC_PlayerJoined>(PlayerJoined);
            GameClient.Subscribe<STC_PlayerDisconnected>(PlayerDisconnected);
            StartLobby();
        }

        private void OnDestroy()
        {
            GameClient.Unsubscribe<STC_PlayerJoined>(PlayerJoined);
            GameClient.Unsubscribe<STC_PlayerDisconnected>(PlayerDisconnected);
        }

        private void StartLobby()
        {
            if (conected) return;
            conected = true;

            AddMe();
        }

        private void AddMe()
        {
            var me = ClientPlayers.Instance.Myself;
            PlaceInLobby(me.id, me.username);
            Debug.LogError(me.id + me.username);

            AddPlayers();
        }

        void AddPlayers()
        {
            foreach (ClientPlayers.Player other in ClientPlayers.Instance.GetOthers())
            {
                string playerName = other.username;
                uint playerId = other.id;
                PlaceInLobby(playerId, playerName);
            }
        }

        private void PlayerJoined(BasePacket p)
        {
            if (!conected) return;

            var pkt = (STC_PlayerJoined)p;
            PlaceInLobby(pkt.playerId, pkt.username);
        }

        private void PlayerDisconnected(BasePacket p)
        {
            if (!conected) return;

            var pkt = (STC_PlayerDisconnected)p;
            RemoveEntry(pkt.playerId);
        }

        private void PlaceInLobby(uint id, string name)
        {
            if (entries.ContainsKey(id))
            {
                Debug.Log("Player with the same id tryed to join: Palyer id " + id);
                return;
            }                    

            GameObject obj = Instantiate(playerEntryPrefab, playerEntryParent);
            obj.GetComponentInChildren<TextMeshProUGUI>().text = name;

            entries.Add(id, obj);
        }

        private void RemoveEntry(uint id)
        {
            if (!entries.TryGetValue(id, out GameObject obj)) return;

            entries.Remove(id);
            Destroy(obj);
        }
    }
}