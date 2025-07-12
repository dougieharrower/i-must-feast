using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudSpawner : MonoBehaviour
{
    [Header("Cloud Settings")]
    public GameObject cloudPrefab;
    public int maxClouds = 5;
    public Vector2 spawnIntervalRange = new Vector2(3f, 7f);
    public Vector2 heightRange = new Vector2(8f, 15f);
    public Vector2 speedRange = new Vector2(0.5f, 1.5f);

    [Header("Movement Settings")]
    public Vector3 moveDirection = Vector3.right; // defaults to +X
    public float gameZoneWidth = 50f;
    public float gameZoneDepth = 50f;

    [Header("Cloud Appearance")]
    [Tooltip("Scale applied uniformly to spawned clouds")]
    public float cloudScale = 1.0f;

    private List<GameObject> activeClouds = new List<GameObject>();
    private Vector3 zoneCenter;

    void Start()
    {
        GameObject ground = GameObject.Find("Ground");
        zoneCenter = ground != null ? ground.transform.position : Vector3.zero;

        StartCoroutine(SpawnCloudsLoop());
    }

    IEnumerator SpawnCloudsLoop()
    {
        while (true)
        {
            if (activeClouds.Count < maxClouds)
            {
                SpawnCloud();
            }

            float waitTime = Random.Range(spawnIntervalRange.x, spawnIntervalRange.y);
            yield return new WaitForSeconds(waitTime);
        }
    }

    void SpawnCloud()
    {
        Vector3 spawnPosition = GetSpawnPosition();
        GameObject cloud = Instantiate(cloudPrefab, spawnPosition, Quaternion.identity);

        cloud.transform.localScale = Vector3.one * cloudScale;

        CloudMover mover = cloud.GetComponent<CloudMover>();
        if (mover == null)
        {
            mover = cloud.AddComponent<CloudMover>();
        }

        mover.Initialize(moveDirection.normalized, speedRange, gameZoneWidth + 10f);
        activeClouds.Add(cloud);
    }

    Vector3 GetSpawnPosition()
    {
        float height = Random.Range(heightRange.x, heightRange.y);
        bool horizontal = Mathf.Abs(moveDirection.x) > Mathf.Abs(moveDirection.z);

        float startOffset = (horizontal ? gameZoneWidth : gameZoneDepth) / 2f + 5f;
        Vector3 startPos = zoneCenter;

        if (horizontal)
        {
            startPos.x += moveDirection.x > 0 ? -startOffset : startOffset;
            startPos.z += Random.Range(-gameZoneDepth / 2f, gameZoneDepth / 2f);
        }
        else
        {
            startPos.z += moveDirection.z > 0 ? -startOffset : startOffset;
            startPos.x += Random.Range(-gameZoneWidth / 2f, gameZoneWidth / 2f);
        }

        startPos.y = height;
        Debug.Log("Spawning cloud at: " + startPos);
        return startPos;
    }
}
