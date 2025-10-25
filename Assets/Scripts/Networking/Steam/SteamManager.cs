using System;
using UnityEngine;

namespace Networking
{
    public class SteamManager : MonoBehaviour
    {
        const uint APP_ID = 480;
        public static SteamManager Instance { get; private set; }
        public bool IsInitialized { get; private set; }

        private void Awake()
        {
            if (Instance && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void Initialize()
        {
            if (IsInitialized || Steamworks.SteamClient.IsValid)
                return;

            try
            {
                Steamworks.SteamClient.Init(APP_ID);
                IsInitialized = true;
                Debug.Log($"Steam initialized. Username: {Steamworks.SteamClient.Name}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to initialize Steam: {e}");
            }
        }

        public static bool TryInitialize()
        {
            if (Steamworks.SteamClient.IsValid)
                return true;

            var go = new GameObject("SteamManager");
            go.AddComponent<SteamManager>().Initialize();
            if (Steamworks.SteamClient.IsValid)
                return true;

            Debug.LogError("Steamworks could not be initialized");
            return false;
        }

        private void Update()
        {
            if (IsInitialized)
                Steamworks.SteamClient.RunCallbacks();
        }

        private void OnApplicationQuit()
        {
            if (Steamworks.SteamClient.IsValid)
                Steamworks.SteamClient.Shutdown();
        }
    }
}