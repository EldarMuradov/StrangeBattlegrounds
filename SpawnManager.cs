using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance;
    private SpawnPoint[] spawnPoints;


    private void Awake()
    {
        Instance = this;
        spawnPoints = GetComponentsInChildren<SpawnPoint>();
    }

    public Transform GetSpawnPoint() 
    {
        return spawnPoints[Random.Range(0, spawnPoints.Length)].transform;
    }
}
