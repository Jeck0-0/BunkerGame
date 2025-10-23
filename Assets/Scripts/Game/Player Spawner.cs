using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField]private GameObject PlayerObj;
    [SerializeField] private GameObject PlayerPlaque;
    [SerializeField] private Transform[] Waypoints;

    private int currentWaypointIndex = 0;
    private int PlayerCount = 0;
    
    public int MaxPlayers = 2;
    void Start()
    {
        SpawnPlayer();
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.S))
        {
            SpawnPlayer();
        }
    }

    void SpawnPlayer()
    {

        if (PlayerCount == MaxPlayers)
        {
            Debug.Log($"Max player of {MaxPlayers} reached");
            return;
        }
        if (Waypoints.Length == 0)
        {
            Debug.LogWarning("No waypoints assigned to PlayerSpawner!");
            return;
        }

        Transform Waypoint = Waypoints[currentWaypointIndex];
        Instantiate(PlayerObj, Waypoint.position, Waypoint.rotation);

        currentWaypointIndex++;

        
        if (currentWaypointIndex >= Waypoints.Length)
        {
            currentWaypointIndex = 0;
        }

        PlayerCount = GameObject.FindGameObjectsWithTag("Player").Length;
        Debug.Log($"{PlayerCount}/{MaxPlayers}");
    }
}
