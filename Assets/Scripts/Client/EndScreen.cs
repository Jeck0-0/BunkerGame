using UnityEngine;
using System.Collections.Generic;
using TMPro;
using Client;
using Networking;
using Packets;

public class EndScreen : MonoBehaviour
{
    [Header("End Screen UI")]
    [SerializeField] private Transform EndScreenArea;
    [SerializeField] private TMP_Text textPrefab;
    [SerializeField] private GameObject EndUI;

    
    public void Awake()
    {
        EndUI.SetActive(false);

        GameClient.Subscribe<STC_GameResault>(Spawn);
    }

    void Spawn(BasePacket p)
    {
        EndUI.SetActive(true);

        STC_GameResault packet = p as STC_GameResault;
        
        foreach (Transform child in EndScreenArea)
        {
            Destroy(child.gameObject);
        }

        foreach (var result in packet._results)
        {
            TMP_Text txt = Instantiate(textPrefab, EndScreenArea);

            txt.text = $"{ClientPlayers.Instance.Get(result.Player).username}  {result.VP}";
        }
    }
}


