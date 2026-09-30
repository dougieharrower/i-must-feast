using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Linq;

[ExecuteAlways]
public class TreelineGeneratorAdvanced : MonoBehaviour
{
    [Header("Tree Prefabs (Drag Your Fir, Oak, Poplar Here)")]
    public GameObject[] treePrefabs;

    [Header("Generation Control")]
    [SerializeField] public int randomSeed = 12345;
    [SerializeField] public bool regenerateOnPlay = false;
    [SerializeField] public int maxPlanted = 40;  // hard cap

    [Header("Ground / Layers")]
    public string groundObjectName = "Ground";
    public LayerMask groundLayer;              // Layer of your Ground mesh (has MeshCollider)
    public float groundRaycastHeight = 100f;   // Height above AABB to raycast down from

    [Header("Tree Plane Mode")]
    public string treePlaneObjectName = "TreePlane"; // child under Ground that covers the area
    public LayerMask treePlaneLayer;                 // Only the TreePlane's layer
    public float groundExclusionRadius = 0.8f;       // reject if this close to Ground collider
    public bool excludeGroundMesh = true;            // turn off to allow trees on Ground

    [Header("NavMesh Avoidance")]
    public bool avoidNavMesh = true;
    public float navMeshAvoidRadius = 1.0f;    // Rejected if within this dist of walkable NavMesh
    public int navMeshAreaMask = NavMesh.AllAreas;

    [Header("Edge (Treeline) Settings")]
    public int edgePointsPerSegment = 20;      // Points per boundary segment
    public int edgeDepthLayers = 3;            // How many inward rings to plant
    public float edgeLayerSpacing = 2.0f;      // Distance inward per layer
    public float edgeJitter = 0.5f;            // Randomize along edge a bit

    [Header("Interior Fill (Blue-Noise)")]
    public bool fillInterior = true;
    public float poissonMinDistance = 3f;      // Min spacing between trees
    public int poissonCandidateTries = 30;     // Standard Bridson tries
    public int poissonTargetCount = 500;       // Upper bound attempt

    [Header("Randomization")]
    public Vector2 randomScaleRange = new Vector2(0.8f, 1.5f);
    public Vector2 randomRotationRange = new Vector2(0f, 360f);
    public Color colorTintMin = new Color(0.8f, 0.8f, 0.8f);
    public Color colorTintMax = new Color(1.2f, 1.2f, 1.2f);

    [Header("Collider Settings")]
    public bool addColliderIfMissing = true;
    public float colliderRadius = 0.5f;
    public float colliderHeight = 4f;

    Mesh _mesh;
    Transform _ground;
    Bounds _meshBounds;

    Transform _treePlane;
    Bounds _planeBounds;

    // internal counter
    int placedCount;

    [ContextMenu("Generate Trees")]
    public void GenerateTrees()
    {
        // Clear previous children
        var toDestroy = new List<GameObject>();
        foreach (Transform child in transform) toDestroy.Add(child.gameObject);
        foreach (var g in toDestroy)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) DestroyImmediate(g);
            else Destroy(g);
#else
            DestroyImmediate(g);
#endif
        }

        if (!TryGetGroundMesh(out _ground, out _mesh))
        {
            Debug.LogError($"Could not find mesh on '{groundObjectName}'. Make sure it has a MeshFilter/MeshCollider.");
            return;
        }

        // Resolve TreePlane
        var planeGO = GameObject.Find(treePlaneObjectName);
        if (planeGO == null)
        {
            Debug.LogError($"TreePlane object '{treePlaneObjectName}' not found!");
            return;
        }
        _treePlane = planeGO.transform;

        _meshBounds = _ground.GetComponent<Renderer>() != null
            ? _ground.GetComponent<Renderer>().bounds
            : new Bounds(_ground.position, Vector3.one * 100f);

        _planeBounds = planeGO.GetComponent<Renderer>() != null
            ? planeGO.GetComponent<Renderer>().bounds
            : new Bounds(planeGO.transform.position, Vector3.one * 100f);

        // 1) Build boundary (concave OK) from Ground mesh
        var boundaryLoops = BuildBoundaryLoops(_mesh, _ground.localToWorldMatrix);
        if (boundaryLoops.Count == 0)
        {
            Debug.LogWarning("No boundary edges detected on ground mesh.");
        }

        // 2) Generate edge treeline (including inward layers) — project onto TreePlane
        var planted = new List<Vector3>();
        foreach (var loop in boundaryLoops)
        {
            var sampled = SampleAlongBoundary(loop, edgePointsPerSegment);
            for (int layer = 0; layer < edgeDepthLayers; layer++)
            {
                float inward = layer * edgeLayerSpacing;
                foreach (var p in sampled)
                {
                    var inwardDir = EstimateInward2D(loop, p);
                    Vector3 candidate = p + inwardDir * inward + RandomXZ(edgeJitter);

                    if (!TryProjectToTreePlane(candidate, out Vector3 hit)) continue;
                    if (excludeGroundMesh && CollidesGround(hit)) continue;
                    if (avoidNavMesh && IsNearNavMesh(hit, navMeshAvoidRadius)) continue;

                    if (IsTooClose(hit, planted, Mathf.Min(poissonMinDistance, 1.5f))) continue;

                    if (SpawnTree(hit)) planted.Add(hit);
                }
            }
        }

        // 3) Interior fill with Poisson-disc on TreePlane, rejecting Ground & NavMesh
        if (fillInterior)
        {
            var interior = PoissonSampleInteriorOnPlane(poissonMinDistance, poissonCandidateTries, poissonTargetCount);
            foreach (var p in interior)
            {
                if (avoidNavMesh && IsNearNavMesh(p, navMeshAvoidRadius)) continue;
                if (IsTooClose(p, planted, poissonMinDistance)) continue;
                if (SpawnTree(p)) planted.Add(p);
            }
        }

        Debug.Log($"Treeline generated! Planted {planted.Count} trees.");
    }

    void Start()
    {
        if (Application.isPlaying && regenerateOnPlay)
            GenerateWithSeed();
    }

    // Call this from the editor buttons
    public void GenerateWithSeed()
    {
#if UNITY_EDITOR
        UnityEditor.Undo.RegisterFullObjectHierarchyUndo(gameObject, "Generate Trees");
#endif
        placedCount = 0;
        Random.InitState(randomSeed);
        GenerateTrees();
    }

    public void ClearGenerated()
    {
#if UNITY_EDITOR
        UnityEditor.Undo.RegisterFullObjectHierarchyUndo(gameObject, "Clear Trees");
#endif
        var toDestroy = new List<GameObject>();
        foreach (Transform child in transform) toDestroy.Add(child.gameObject);
#if UNITY_EDITOR
        foreach (var g in toDestroy) if (!Application.isPlaying) DestroyImmediate(g); else Destroy(g);
#else
        foreach (var g in toDestroy) DestroyImmediate(g);
#endif
    }

    // Make SpawnTree return bool and honor maxPlanted
    bool SpawnTree(Vector3 pos)
    {
        if (placedCount >= maxPlanted) return false;
        if (treePrefabs == null || treePrefabs.Length == 0) return false;

        var prefab = treePrefabs[Random.Range(0, treePrefabs.Length)];
        var tree = Instantiate(prefab, pos, Quaternion.identity, transform);

        float rotY = Random.Range(randomRotationRange.x, randomRotationRange.y);
        tree.transform.rotation = Quaternion.Euler(0, rotY, 0);
        float scl = Random.Range(randomScaleRange.x, randomScaleRange.y);
        tree.transform.localScale = Vector3.one * scl;
        ApplyRandomTint(tree);

        if (addColliderIfMissing && tree.GetComponent<Collider>() == null)
        {
            var col = tree.AddComponent<CapsuleCollider>();
            col.center = Vector3.zero;
            col.radius = colliderRadius;
            col.height = colliderHeight;
            col.isTrigger = false;
        }

        placedCount++;
        return true;
    }

#if UNITY_EDITOR
    // Bakes current children into a new parent and disables this generator
    public void BakeToNewParent()
    {
        var baked = new GameObject($"TreeLine_Baked_{randomSeed}");
        UnityEditor.Undo.RegisterCreatedObjectUndo(baked, "Bake Trees");
        baked.transform.SetPositionAndRotation(transform.position, transform.rotation);
        baked.transform.localScale = transform.localScale;

        var movers = new List<Transform>();
        foreach (Transform c in transform) movers.Add(c);
        foreach (var c in movers) c.SetParent(baked.transform, true);

        UnityEditor.GameObjectUtility.SetStaticEditorFlags(
            baked, UnityEditor.StaticEditorFlags.BatchingStatic |
                   UnityEditor.StaticEditorFlags.OccludeeStatic |
                   UnityEditor.StaticEditorFlags.OccluderStatic);

        regenerateOnPlay = false;
        enabled = false;

        UnityEditor.Selection.activeGameObject = baked;
        UnityEditor.EditorUtility.DisplayDialog("Treeline", "Baked current trees into a new parent.\nThe generator was disabled.", "OK");
    }
#endif

    #region Mesh / Boundary

    bool TryGetGroundMesh(out Transform ground, out Mesh mesh)
    {
        ground = null; mesh = null;

        var go = GameObject.Find(groundObjectName);
        if (go == null) return false;

        ground = go.transform;

        var mf = go.GetComponent<MeshFilter>();
        if (mf != null && mf.sharedMesh != null)
        {
            mesh = mf.sharedMesh;
            return true;
        }

        var mc = go.GetComponent<MeshCollider>();
        if (mc != null && mc.sharedMesh != null)
        {
            mesh = mc.sharedMesh;
            return true;
        }

        return false;
    }

    // Returns loops of world-space points describing the outer boundary (edges used by only one triangle)
    List<List<Vector3>> BuildBoundaryLoops(Mesh mesh, Matrix4x4 localToWorld)
    {
        var verts = mesh.vertices;
        var tris = mesh.triangles;

        var edgeCount = new Dictionary<(int, int), (int a, int b, int usedBy)>();

        for (int i = 0; i < tris.Length; i += 3)
        {
            int a = tris[i];
            int b = tris[i + 1];
            int c = tris[i + 2];
            CountEdge(a, b);
            CountEdge(b, c);
            CountEdge(c, a);
        }

        void CountEdge(int i1, int i2)
        {
            var key = i1 < i2 ? (i1, i2) : (i2, i1);
            if (edgeCount.TryGetValue(key, out var v))
                edgeCount[key] = (v.a, v.b, v.usedBy + 1);
            else
                edgeCount[key] = (key.Item1, key.Item2, 1);
        }

        var boundaryEdges = edgeCount.Values.Where(e => e.usedBy == 1).ToList();
        if (boundaryEdges.Count == 0) return new List<List<Vector3>>();

        var adj = new Dictionary<int, List<int>>();
        void AddAdj(int x, int y)
        {
            if (!adj.TryGetValue(x, out var list))
            {
                list = new List<int>();
                adj[x] = list;
            }
            if (!list.Contains(y)) list.Add(y);
        }

        foreach (var e in boundaryEdges)
        {
            AddAdj(e.a, e.b);
            AddAdj(e.b, e.a);
        }

        var visited = new HashSet<(int, int)>();
        var loops = new List<List<Vector3>>();

        foreach (var start in adj.Keys)
        {
            foreach (var n in adj[start])
            {
                if (visited.Contains((start, n))) continue;

                var loopIdx = new List<int>();
                int curr = start;
                int next = n;

                loopIdx.Add(curr);
                while (true)
                {
                    visited.Add((curr, next));
                    loopIdx.Add(next);

                    var neigh = adj[next];
                    int candidate = -1;
                    foreach (var t in neigh)
                    {
                        if (t == curr) continue;
                        if (!visited.Contains((next, t)))
                        {
                            candidate = t;
                            break;
                        }
                    }

                    if (candidate == -1) break;

                    curr = next;
                    next = candidate;

                    if (next == loopIdx[0]) { loopIdx.Add(next); break; }
                }

                if (loopIdx.Count >= 3)
                {
                    var world = loopIdx.Select(idx => localToWorld.MultiplyPoint3x4(verts[idx])).ToList();
                    if (SignedAreaXZ(world) < 0f) world.Reverse();
                    loops.Add(world);
                }
            }
        }

        loops = loops.Where(l => l.Count > 3 && Mathf.Abs(SignedAreaXZ(l)) > 0.01f).ToList();
        return loops;
    }

    float SignedAreaXZ(List<Vector3> poly)
    {
        double area = 0;
        for (int i = 0; i < poly.Count - 1; i++)
        {
            var p = poly[i];
            var q = poly[i + 1];
            area += (double)p.x * q.z - (double)q.x * p.z;
        }
        return (float)(0.5 * area);
    }

    List<Vector3> SampleAlongBoundary(List<Vector3> loop, int pointsPerSegment)
    {
        var pts = new List<Vector3>();
        for (int i = 0; i < loop.Count - 1; i++)
        {
            var a = loop[i];
            var b = loop[i + 1];
            for (int k = 0; k < pointsPerSegment; k++)
            {
                float t = pointsPerSegment == 1 ? 0f : (float)k / (pointsPerSegment - 1);
                var p = Vector3.Lerp(a, b, t);
                pts.Add(p);
            }
        }
        return pts;
    }

    Vector3 EstimateInward2D(List<Vector3> loop, Vector3 pointOnLoop)
    {
        float best = float.MaxValue;
        Vector3 bestDir = Vector3.zero;

        for (int i = 0; i < loop.Count - 1; i++)
        {
            var a = loop[i];
            var b = loop[i + 1];
            var seg = b - a;
            var ap = pointOnLoop - a;
            float t = Mathf.Clamp01(Vector3.Dot(ap, seg) / Mathf.Max(seg.sqrMagnitude, 1e-6f));
            var closest = a + seg * t;
            float d = (pointOnLoop - closest).sqrMagnitude;
            if (d < best)
            {
                best = d;
                Vector3 right = new Vector3(seg.z, 0, -seg.x).normalized;
                bestDir = -right; // inward
            }
        }
        return bestDir.sqrMagnitude > 0 ? bestDir : Vector3.forward;
    }

    #endregion

    #region Interior Sampling (TreePlane)

    List<Vector3> PoissonSampleInteriorOnPlane(float minDist, int k, int targetCount)
    {
        var result = new List<Vector3>();

        Vector2 min = new Vector2(_planeBounds.min.x, _planeBounds.min.z);
        Vector2 max = new Vector2(_planeBounds.max.x, _planeBounds.max.z);

        float cell = minDist / Mathf.Sqrt(2);
        int cols = Mathf.CeilToInt((max.x - min.x) / cell);
        int rows = Mathf.CeilToInt((max.y - min.y) / cell);

        int[,] grid = new int[cols, rows];
        for (int i = 0; i < cols; i++)
            for (int j = 0; j < rows; j++)
                grid[i, j] = -1;

        var active = new List<Vector3>();

        int GridX(Vector3 p) => Mathf.Clamp((int)((p.x - min.x) / cell), 0, cols - 1);
        int GridY(Vector3 p) => Mathf.Clamp((int)((p.z - min.y) / cell), 0, rows - 1);

        // Seed
        for (int tries = 0; tries < 50; tries++)
        {
            var seedXZ = new Vector3(Random.Range(min.x, max.x), _planeBounds.max.y + groundRaycastHeight, Random.Range(min.y, max.y));
            if (!TryProjectToTreePlane(seedXZ, out var seed)) continue;
            if (excludeGroundMesh && CollidesGround(seed)) continue;
            if (avoidNavMesh && IsNearNavMesh(seed, navMeshAvoidRadius)) continue;

            result.Add(seed);
            active.Add(seed);
            grid[GridX(seed), GridY(seed)] = 0;
            break;
        }

        if (active.Count == 0) return result;

        while (active.Count > 0 && result.Count < targetCount)
        {
            int idx = Random.Range(0, active.Count);
            var center = active[idx];
            bool found = false;

            for (int i = 0; i < k; i++)
            {
                float r = Random.Range(minDist, 2f * minDist);
                float ang = Random.Range(0f, Mathf.PI * 2f);
                var candidate = center + new Vector3(Mathf.Cos(ang), 0, Mathf.Sin(ang)) * r;
                candidate.y = _planeBounds.max.y + groundRaycastHeight;

                if (!TryProjectToTreePlane(candidate, out var hit)) continue;
                if (excludeGroundMesh && CollidesGround(hit)) continue;
                if (avoidNavMesh && IsNearNavMesh(hit, navMeshAvoidRadius)) continue;

                bool ok = true;
                int gx = GridX(hit), gy = GridY(hit);
                int rcheck = 2;
                for (int xx = Mathf.Max(0, gx - rcheck); xx <= Mathf.Min(cols - 1, gx + rcheck) && ok; xx++)
                for (int yy = Mathf.Max(0, gy - rcheck); yy <= Mathf.Min(rows - 1, gy + rcheck) && ok; yy++)
                {
                    int id = grid[xx, yy];
                    if (id == -1) continue;
                    if ((result[id] - hit).sqrMagnitude < minDist * minDist) ok = false;
                }
                if (!ok) continue;

                grid[gx, gy] = result.Count;
                result.Add(hit);
                active.Add(hit);
                found = true;
                break;
            }

            if (!found) active.RemoveAt(idx);
        }

        return result;
    }

    #endregion

    #region Queries / Utility

    bool TryProjectToTreePlane(Vector3 xzStart, out Vector3 hitPoint)
    {
        // Raycast downward onto the TreePlane layer
        Ray ray = new Ray(new Vector3(xzStart.x, _planeBounds.max.y + groundRaycastHeight, xzStart.z), Vector3.down);
        if (Physics.Raycast(ray, out var hit, Mathf.Infinity, treePlaneLayer, QueryTriggerInteraction.Ignore))
        {
            hitPoint = hit.point;
            return true;
        }
        hitPoint = default;
        return false;
    }

    bool CollidesGround(Vector3 p)
    {
        // Reject if inside/touching the Ground collider (keeps trees off the walkable mesh)
        return Physics.CheckSphere(p + Vector3.up * 0.1f, groundExclusionRadius, groundLayer, QueryTriggerInteraction.Ignore);
    }

    bool IsNearNavMesh(Vector3 p, float radius)
    {
        if (!NavMesh.SamplePosition(p, out var hit, radius, navMeshAreaMask)) return false;
        Vector2 a = new Vector2(p.x, p.z);
        Vector2 b = new Vector2(hit.position.x, hit.position.z);
        return Vector2.Distance(a, b) <= radius;
    }

    bool IsTooClose(Vector3 p, List<Vector3> list, float minDist)
    {
        float minSqr = minDist * minDist;
        for (int i = 0; i < list.Count; i++)
            if ((list[i] - p).sqrMagnitude < minSqr) return true;
        return false;
    }

    Vector3 RandomXZ(float magnitude)
    {
        if (magnitude <= 0) return Vector3.zero;
        float a = Random.Range(0f, Mathf.PI * 2f);
        float r = Random.Range(0f, magnitude);
        return new Vector3(Mathf.Cos(a) * r, 0, Mathf.Sin(a) * r);
    }

    void ApplyRandomTint(GameObject tree)
    {
        var rend = tree.GetComponentInChildren<Renderer>();
        if (!rend) return;

        var props = new MaterialPropertyBlock();
        rend.GetPropertyBlock(props);

        Color c = new Color(
            Random.Range(colorTintMin.r, colorTintMax.r),
            Random.Range(colorTintMin.g, colorTintMax.g),
            Random.Range(colorTintMin.b, colorTintMax.b)
        );

        props.SetColor("_Color", c);
        rend.SetPropertyBlock(props);
    }

    // Kept for completeness; currently unused guard.
    bool IsInsideMesh(Vector3 p) => true;

    #endregion
}
