using UnityEngine;

public class GroundTiler : MonoBehaviour
{
    [Header("Tile Prefabs (Drop your 4x4 prefabs here)")]
    public GameObject[] tilePrefabs;

    [Header("Tiling Area (in tiles)")]
    public int tilesWide = 10;
    public int tilesDeep = 10;

    [Header("Tile Size (usually 4 for 4x4 prefabs)")]
    public float tileSize = 4f;

    [Header("Random Rotation")]
    public bool randomRotate = true;

    void Start()
    {
        if (tilePrefabs.Length == 0)
        {
            Debug.LogWarning("No tile prefabs assigned!");
            return;
        }

        for (int x = 0; x < tilesWide; x++)
        {
            for (int z = 0; z < tilesDeep; z++)
            {
                // Choose a random prefab
                GameObject prefab = tilePrefabs[Random.Range(0, tilePrefabs.Length)];

                // Calculate position
                Vector3 position = new Vector3(x * tileSize, 0f, z * tileSize) + transform.position;

                // Optional rotation
                Quaternion rotation = Quaternion.identity;
                if (randomRotate)
                {
                    float angle = 90f * Random.Range(0, 4); // 0, 90, 180, 270
                    rotation = Quaternion.Euler(0f, angle, 0f);
                }

                // Spawn tile
                Instantiate(prefab, position, rotation, transform);
            }
        }
    }
}
