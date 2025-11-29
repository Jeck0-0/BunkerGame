using Client;
using Networking;
using Packets;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Server
{
    public class HostLobbyManager : MonoBehaviour
    {
        [SerializeField] GameObject playerEntryPrefab;
        [SerializeField] Transform playerEntryParent;
        [SerializeField] Button gameStartButton;

        private Dictionary<uint, GameObject> entries = new();

        void Start()
        {
            GameServer.Instance.Create(6);
            ClientPlayers.Instance.OnSpotReceived += AddMeAfterJoin;

            GameServer.OnPlayerDisconnected += PlayerDisconnected;
            ServerPlayers.Instance.OnPlayerQuit += PlayerQuit;
            GameClient.Subscribe<STC_PlayerJoined>(PlayerConnected);

            gameStartButton.onClick.AddListener(StartGame);
        }

        void OnDestroy()
        {
            ClientPlayers.Instance.OnSpotReceived -= AddMeAfterJoin;
            GameServer.OnPlayerDisconnected -= PlayerDisconnected;
            GameClient.Unsubscribe<STC_PlayerJoined>(PlayerConnected);

            ServerPlayers.Instance.OnPlayerQuit -= PlayerQuit;
        }
        private void AddMeAfterJoin()
        {
            AddMe();
            ClientPlayers.Instance.OnSpotReceived -= AddMeAfterJoin; // only once
        }

        void AddMe()
        {
            var me = ClientPlayers.Instance.Myself;
            PlaceInLobby(me.id, me.username, true);
        }

        private void PlayerConnected(BasePacket p)
        {
            var pkt = (STC_PlayerJoined)p;

            if (pkt.playerId == ClientPlayers.Instance.Myself.id) return; // Don't add myself here
            PlaceInLobby(pkt.playerId, pkt.username, false);
        }

        private void PlayerDisconnected(uint id)
        {
            RemoveEntry(id);
        }

        private void PlayerQuit(ServerPlayers.Player p)
        {
            RemoveEntry(p.id);
        }

        private void PlaceInLobby(uint id, string name, bool host = false)
        {
            if (entries.ContainsKey(id))
            {
                Debug.Log("Player with the same id tryed to join: Palyer id " + id);
                return;
            }

            GameObject entry = Instantiate(playerEntryPrefab, playerEntryParent);
            var ui = entry.GetComponent<PlayerEntryUI>();
            ui.InitializeButton(name, host);

            if (!host)
            {
                ui.ButtonReference.onClick.RemoveAllListeners();
                ui.ButtonReference.onClick.AddListener(() => KickPlayer(id));
            }

            entries.Add(id, entry);
        }

        private void RemoveEntry(uint id)
        {
            if (!entries.TryGetValue(id, out GameObject entry)) return;

            entries.Remove(id);
            Destroy(entry);
        }

        public void KickPlayer(uint id)
        {
            RemoveEntry(id);
        }

        public void StartGame()
        {
            GameServer.SendToAll(new STC_GameStart());
        }
    }
}