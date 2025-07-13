using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BushController : MonoBehaviour
{
    [Header("Bush Settings")]
    public GameObject[] bushPrefabs;
    public int minBushes = 5;
    public int maxBushes = 15;
    public int overrideBushCount = -1; // -1 means ignore

    [Header("Placement Settings")]
    public float scaleVariance = 0.2f; // e.g. 0.2 means scale between 0.9x and 1.1x
    public bool allowOverlap = false;
    public float padding = 1.5f; // min distance between bushes if overlap is false

    [Header("Placement Zone")]
    public Vector3 areaCenter = Vector3.zero;
    public Vector2 areaSize = new Vector2(30f, 30f); // XZ area

    private List<Vector3> placedPositions = new List<Vector3>();

    void Start()
    {
        int count = overrideBushCount > 0 ? overrideBushCount : Random.Range(minBushes, maxBushes + 1);
        SpawnBushes(count);
    }

void SpawnBushes(int count)
{
    int attempts = 0;
    int spawned = 0;

    while (spawned < count && attempts < count * 10)
    {
        Vector3 position = GetRandomPositionOnNavMesh();

        if (!allowOverlap && !IsPositionValid(position))
        {
            attempts++;
            continue;
        }

        GameObject prefab = bushPrefabs[Random.Range(0, bushPrefabs.Length)];
        Quaternion randomRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        GameObject bush = Instantiate(prefab, position, randomRotation);

        // Apply uniform scale variance
        float scaleFactor = 1f + Random.Range(-scaleVariance, scaleVariance);
        bush.transform.localScale = Vector3.one * scaleFactor;

        // Ensure the bush sits just slightly in the ground using bounds
        Renderer rend = bush.GetComponentInChildren<Renderer>();
        if (rend != null)
        {
            float bottomY = rend.bounds.min.y;
            float bushY = bush.transform.position.y;
            float sinkAmount = bottomY - bushY + 0.05f; // sink slightly (5 cm)
            bush.transform.position -= new Vector3(0, sinkAmount, 0);
        }
        else
        {
            Debug.LogWarning("No Renderer found on bush prefab.");
        }

        // Ensure trigger collider is active
        Collider col = bush.GetComponent<Collider>();
        if (col is BoxCollider box && !box.isTrigger)
        {
            box.isTrigger = true;
        }

        // Log position for debugging
        Debug.Log($"Bush {spawned + 1} spawned at {bush.transform.position} with scale {bush.transform.localScale}");

        placedPositions.Add(bush.transform.position);
        spawned++;
    }

    Debug.Log($"Spawned {spawned} bushes after {attempts} attempts.");
}



    Vector3 GetRandomPositionOnNavMesh()
    {
        Vector3 randomPoint = areaCenter + new Vector3(
            Random.Range(-areaSize.x / 2f, areaSize.x / 2f),
            10f, // height to start ray
            Random.Range(-areaSize.y / 2f, areaSize.y / 2f));

        if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 20f, NavMesh.AllAreas))
        {
            return hit.position;
        }

        return randomPoint; // fallback
    }

    bool IsPositionValid(Vector3 position)
    {
        foreach (var placed in placedPositions)
        {
            if (Vector3.Distance(placed, position) < padding)
                return false;
        }
        return true;
    }
}
