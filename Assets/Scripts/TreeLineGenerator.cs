using UnityEngine;
using System.Collections.Generic;



public class TreelineGenerator : MonoBehaviour
{
    [Header("Tree Prefabs (Drag Your Fir, Oak, Poplar Here)")]
    public GameObject[] treePrefabs;

    [Header("Placement Settings")]
    public int treesPerEdge = 30;
    public int depthLayers = 4;
    public float layerSpacing = 2.5f;

    [Header("Randomization")]
    public Vector2 randomScaleRange = new Vector2(0.8f, 1.5f);
    public Vector2 randomRotationRange = new Vector2(0f, 360f);
    public Color colorTintMin = new Color(0.8f, 0.8f, 0.8f);
    public Color colorTintMax = new Color(1.2f, 1.2f, 1.2f);

    [Header("References")]
    public string groundObjectName = "Ground";

    [ContextMenu("Generate Trees")]
    public void GenerateTrees()
    {
        // Remove old trees
        foreach (Transform child in transform)
        {
            DestroyImmediate(child.gameObject);
        }

        GameObject ground = GameObject.Find(groundObjectName);
        if (ground == null)
        {
            Debug.LogError($"Ground object '{groundObjectName}' not found!");
            return;
        }

        Bounds bounds = ground.GetComponent<Renderer>().bounds;
        Vector3 min = bounds.min;
        Vector3 max = bounds.max;

        List<Vector3> edgePoints = new List<Vector3>();

        // Generate points along edges (clockwise)
        for (int i = 0; i < treesPerEdge; i++)
        {
            float t = (float)i / (treesPerEdge - 1);
            edgePoints.Add(Vector3.Lerp(new Vector3(min.x, 0, min.z), new Vector3(max.x, 0, min.z), t)); // Bottom edge
            edgePoints.Add(Vector3.Lerp(new Vector3(max.x, 0, min.z), new Vector3(max.x, 0, max.z), t)); // Right edge
            edgePoints.Add(Vector3.Lerp(new Vector3(max.x, 0, max.z), new Vector3(min.x, 0, max.z), t)); // Top edge
            edgePoints.Add(Vector3.Lerp(new Vector3(min.x, 0, max.z), new Vector3(min.x, 0, min.z), t)); // Left edge
        }

        // Place trees
        foreach (Vector3 edgePoint in edgePoints)
        {
            for (int layer = 0; layer < depthLayers; layer++)
            {
                Vector3 offsetDir = (edgePoint - bounds.center).normalized;
                float offsetDistance = layer * layerSpacing;
                Vector3 position = edgePoint + offsetDir * offsetDistance;

                // Slight Y offset to ground trees visually
                position.y = ground.transform.position.y;

                GameObject treePrefab = treePrefabs[Random.Range(0, treePrefabs.Length)];
                GameObject tree = Instantiate(treePrefab, position, Quaternion.identity, transform);

                // Randomize rotation, scale
                float randomRotation = Random.Range(randomRotationRange.x, randomRotationRange.y);
                tree.transform.Rotate(Vector3.up, randomRotation);

                float randomScale = Random.Range(randomScaleRange.x, randomScaleRange.y);
                tree.transform.localScale = Vector3.one * randomScale;

                // Randomize color tint
                ApplyRandomTint(tree);

                // Add collider to block player/NPCs
                if (tree.GetComponent<Collider>() == null)
                {
                    var collider = tree.AddComponent<CapsuleCollider>();
                    collider.center = Vector3.zero;
                    collider.radius = 0.5f;
                    collider.height = 4f;
                    collider.isTrigger = false;
                }
            }
        }

        Debug.Log("Treeline generated!");
    }

void Start()
{
    GenerateTrees();  // Auto-generates trees on game start
}

    void ApplyRandomTint(GameObject tree)
    {
        Renderer renderer = tree.GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            MaterialPropertyBlock props = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(props);

            Color randomColor = new Color(
                Random.Range(colorTintMin.r, colorTintMax.r),
                Random.Range(colorTintMin.g, colorTintMax.g),
                Random.Range(colorTintMin.b, colorTintMax.b)
            );

            props.SetColor("_Color", randomColor);
            renderer.SetPropertyBlock(props);
        }
    }
}
