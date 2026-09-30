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
    private List<GameObject> spawnedVictims = new List<GameObject>();

    // Requested spawn count from the Inspector. Use SpawnedCount for the
    // actual number of victims that exist, since NavMesh sampling can fail
    // to place every requested victim.
    public int TotalVictims => numberToSpawn;
    public int SpawnedCount { get; private set; }
    public IReadOnlyList<GameObject> SpawnedVictims => spawnedVictims;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        // Spawn during Awake (not Start) so every victim exists before any
        // other script's Start() runs, e.g. FeastGameManager reading
        // SpawnedCount to set the win condition.
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
                spawnedVictims.Add(victim);
                spawned++;
            }
        }

        SpawnedCount = spawned;

        if (spawned < numberToSpawn)
        {
            Debug.LogWarning($"VictimSpawner: Only spawned {spawned} victims out of requested {numberToSpawn}. Win condition will use the actual spawned count.");
        }
        else
        {
            Debug.Log($"Spawned {spawned} victims out of requested {numberToSpawn}");
        }
    }
}
