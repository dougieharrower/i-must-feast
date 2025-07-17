using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class VictimSpawner : MonoBehaviour
{
    public static VictimSpawner Instance;

    [Header("Victim Spawn Settings")]
    public GameObject victimPrefab;
    public int numberToSpawn = 5;
    public float spawnRadius = 25f;
    public float spawnPadding = 3f;

    [Header("NavMesh Settings")]
    public int maxAttempts = 30;

    private List<Vector3> spawnPoints = new List<Vector3>();
    public int TotalVictims => numberToSpawn;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        SpawnVictims();
    }

    void SpawnVictims()
    {
        if (victimPrefab == null)
        {
            Debug.LogWarning("VictimSpawner: No victimPrefab assigned!");
            return;
        }

        int spawned = 0;
        int attempts = 0;

        while (spawned < numberToSpawn && attempts < numberToSpawn * maxAttempts)
        {
            attempts++;

            Vector3 randomPos = transform.position + Random.insideUnitSphere * spawnRadius;
            randomPos.y = transform.position.y;

            if (NavMesh.SamplePosition(randomPos, out NavMeshHit hit, 3f, NavMesh.AllAreas))
            {
                Vector3 finalPos = hit.position;

                bool overlaps = false;
                foreach (Vector3 pos in spawnPoints)
                {
                    if (Vector3.Distance(pos, finalPos) < spawnPadding)
                    {
                        overlaps = true;
                        break;
                    }
                }

                if (overlaps) continue;

                GameObject victim = Instantiate(victimPrefab, finalPos, Quaternion.identity);
                spawnPoints.Add(finalPos);
                spawned++;
            }
        }

        Debug.Log($"Spawned {spawned} victims out of requested {numberToSpawn}");
    }
}
