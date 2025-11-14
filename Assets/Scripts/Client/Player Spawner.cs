using UnityEngine;
using System.Linq;
using Server;
using Client;
public class PlayerSpawner : MonoBehaviour
{
    [SerializeField]private GameObject PlayerObj;
    //[SerializeField] private GameObject PlayerPlaque;
    [SerializeField] private Transform[] Spawnpoints;

    public int MaxPlayers = 6;
    void Start()
    {
        SpawnAllPlayersFromServer();
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.S))
        {
            SpawnAllPlayersFromServer();
        }
    }

    void SpawnAllPlayersFromServer()
    {
       if (Spawnpoints.Length == 0)
       {
           Debug.LogWarning("No waypoints assigned to PlayerSpawner!");
           return;
       }

        GameObject[] existingPlayers = GameObject.FindGameObjectsWithTag("Player");

        for (int i = 0; i < existingPlayers.Length; i++)
        {
            Destroy(existingPlayers[i]);

        }

        ServerPlayers.Player[] players = ServerPlayers
            .GetAll()
            .OrderBy(p => p.id)
            .ToArray();

        int SpawnCount = 0;

        for(int i = 0;i < players.Length;i++)
        {
            if (i >= MaxPlayers)
            {
                Debug.Log("Max player of " + MaxPlayers + " reached");
                break;
            }

            if (i >= Spawnpoints.Length)
            {
                Debug.LogWarning("Not enough spawnpoints for all players!");
                break;
            }
            Transform waypoint = Spawnpoints[i];
            Instantiate(PlayerObj, waypoint.position, waypoint.rotation);

            SpawnCount++;
        }

        Debug.LogError(SpawnCount + "/" + MaxPlayers + " players spawned from server data.");
    }
}
