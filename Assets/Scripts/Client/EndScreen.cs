using UnityEngine;
using Server;
using System.Collections.Generic;
using TMPro;

public class EndScreen : MonoBehaviour
{
    [Header("End Screen UI")]
    [SerializeField] private Transform EndScreenArea;
    [SerializeField] private TMP_Text textPrefab;
    [SerializeField] private GameObject EndUI;

    private List<PlayerResult> _results = new List<PlayerResult>();

    public void Start()
    {
        EndUI.SetActive(false);
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            GetPlayerPoints();
        }
    }

    public void GetPlayerPoints()
    {
        EndUI.SetActive(true);

        _results.Clear();

        foreach (var player in ServerPlayers.GetAll())
        {
            int vp = VPCalculator.Instance.CalculatePlayerVP(player);

            _results.Add(new PlayerResult { Player = player,
                                            VP = vp });
        }
        Orderpoints();

        Spawn();
    }

    void Orderpoints()
    {
        _results.Sort((a, b) => b.VP.CompareTo(a.VP));
    }

    void Spawn()
    {
        foreach (Transform child in EndScreenArea)
        {
            Destroy(child.gameObject);
        }

        foreach (var result in _results)
        {
            TMP_Text txt = Instantiate(textPrefab, EndScreenArea);

            txt.text = $"{result.Player.username}  {result.VP}";
        }
    }
}

public struct PlayerResult
{
    public ServerPlayers.Player Player;
    public int VP;
}
