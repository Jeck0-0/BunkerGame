using System;
using System.Threading.Tasks;
using Steamworks;
using Steamworks.Data;
using UnityEngine;

namespace Networking
{
    public static class SteamLobby
    {
        public static Lobby? CurrentLobby { get; private set; }
        public static bool IsInLobby => CurrentLobby != null;
        public static bool IsOwner => IsInLobby && CurrentLobby.Value.Owner.Id == Steamworks.SteamClient.SteamId;

        public static void Initialize()
        {
            SteamMatchmaking.OnLobbyCreated += LobbyCreatedCallback;
            SteamMatchmaking.OnLobbyEntered += LobbyEnteredCallback;
            SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoinedCallback;
            SteamMatchmaking.OnLobbyMemberLeave += OnLobbyMemberLeaveCallback;
            SteamMatchmaking.OnLobbyDataChanged += OnLobbyDataChangedCallback;
            SteamMatchmaking.OnLobbyInvite += OnLobbyInviteCallback;
            SteamFriends.OnGameLobbyJoinRequested += OnGameLobbyJoinRequestedCallback;
            Application.quitting += OnDestroy;
        }


        public static async Task<bool> Create(int maxMembers)
        {
            if (!SteamManager.TryInitialize())
            {
                Debug.LogError("Steam not initialized");
                return false;
            }

            try
            {
                var result = await SteamMatchmaking.CreateLobbyAsync(maxMembers);

                if (result == null)
                {
                    Debug.LogError("Could not create lobby");
                    return false;
                }

                CurrentLobby = result;
                CurrentLobby.Value.SetPublic();
                CurrentLobby.Value.SetJoinable(true);
                CurrentLobby.Value.SetData("game_version", Application.version);

                //OVERLAY
                SteamFriends.OpenGameInviteOverlay(CurrentLobby.Value.Id);

                Debug.Log($"Lobby created ({CurrentLobby.Value.Id})");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error creating lobby: {e.Message}");
                return false;
            }
        }

        public static async Task<bool> Join(SteamId lobbyId)
        {
            try
            {
                var lobby = await SteamMatchmaking.JoinLobbyAsync(lobbyId);
                if (lobby == null)
                {
                    Debug.LogError("Failed to join lobby!");
                    return false;
                }

                CurrentLobby = lobby;
                Debug.Log($"Joined lobby ({CurrentLobby.Value.Id})");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error joining lobby: {e.Message}");
                return false;
            }
        }

        public static void Leave()
        {
            if (IsInLobby)
            {
                CurrentLobby!.Value.Leave();
                CurrentLobby = null;
                Debug.Log("Left lobby");
            }
        }

        public static void OpenInviteOverlay()
        {
            if (IsInLobby)
                SteamFriends.OpenGameInviteOverlay(CurrentLobby!.Value.Id);
        }

        public static void OpenJoinOverlay()
        {
            SteamFriends.OpenOverlay("friends");
        }




        private static void LobbyCreatedCallback(Result result, Lobby lobby)
        {
            if (result != Result.OK)
            {
                Debug.LogError($"Lobby creation failed: {result}");
                return;
            }

            Debug.Log($"Lobby created callback - ID: {lobby.Id}");
        }

        private static void LobbyEnteredCallback(Lobby lobby)
        {
            Debug.Log($"[{lobby.Id}] Entered {lobby.Owner.Name}'s lobby ({lobby.MemberCount}/{lobby.MaxMembers})");
            foreach (var member in lobby.Members)
                Debug.Log($"Member: {member.Name} (ID: {member.Id})");
        }

        private static void OnLobbyMemberJoinedCallback(Lobby lobby, Friend friend)
        {
            Debug.Log($"{friend.Name} joined the lobby");

            // Send a chat message or trigger an event
            // NetworkManager.Instance?.OnPlayerJoined(friend.Id);
        }

        private static void OnLobbyMemberLeaveCallback(Lobby lobby, Friend friend)
        {
            Debug.Log($"{friend.Name} left the lobby");

            // NetworkManager.Instance?.OnPlayerLeft(friend.Id);
        }

        private static void OnLobbyDataChangedCallback(Lobby lobby)
        {
            Debug.Log("Lobby data changed");

            // Check for game state changes
            string gameState = lobby.GetData("game_state");
            if (!string.IsNullOrEmpty(gameState))
            {
                Debug.Log($"Game state: {gameState}");
            }
        }

        private static void OnLobbyInviteCallback(Friend friend, Lobby lobby)
        {
            Debug.Log($"Invited to lobby by {friend.Name}");
            // The Steam overlay will handle showing the invite
        }

        private static async void OnGameLobbyJoinRequestedCallback(Lobby lobby, SteamId friendId)
        {
            Debug.Log($"Join requested for lobby {lobby.Id} from friend {friendId}");

            // Automatically join when user clicks "Join Game" in Steam
            await Join(lobby.Id);
        }

        private static void OnDestroy()
        {
            SteamMatchmaking.OnLobbyCreated -= LobbyCreatedCallback;
            SteamMatchmaking.OnLobbyEntered -= LobbyEnteredCallback;
            SteamMatchmaking.OnLobbyMemberJoined -= OnLobbyMemberJoinedCallback;
            SteamMatchmaking.OnLobbyMemberLeave -= OnLobbyMemberLeaveCallback;
            SteamMatchmaking.OnLobbyDataChanged -= OnLobbyDataChangedCallback;
            SteamMatchmaking.OnLobbyInvite -= OnLobbyInviteCallback;
            SteamFriends.OnGameLobbyJoinRequested -= OnGameLobbyJoinRequestedCallback;
            Application.quitting -= OnDestroy;
        }

    }
}
