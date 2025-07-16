using UnityEngine;
using UnityEngine.AI;

public class GnomeSpawner : MonoBehaviour
{
    public GameObject gnomePrefab;
    public int numberToSpawn = 3;
    public float spawnRadius = 10f;
    public Transform spawnCenter;

    void Start()
    {
        SpawnGnomes();
    }

    void SpawnGnomes()
    {
        for (int i = 0; i < numberToSpawn; i++)
        {
            Vector3 randomOffset = Random.insideUnitSphere * spawnRadius;
            randomOffset.y = 0;

            Vector3 spawnPosition = spawnCenter ? spawnCenter.position + randomOffset : transform.position + randomOffset;

            // Sample position on the NavMesh
            if (NavMesh.SamplePosition(spawnPosition, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                Instantiate(gnomePrefab, hit.position, Quaternion.identity);
            }
        }
    }
}
