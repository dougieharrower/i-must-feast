using UnityEngine;

public class SunlightDetector : MonoBehaviour
{
    [Header("Detection Settings")]
    public Light sunLight;             // Reference to your directional light
    public float checkDistance = 100f; // How far to check (long enough to reach from sun's direction)
    public LayerMask coverLayer;       // Only detect objects that can block the sun

    [Header("Debug")]
    public bool isInShade = false;

    void Update()
    {
        CheckForShade();
    }

    void CheckForShade()
    {
        if (sunLight == null)
        {
            Debug.LogWarning("SunlightDetector: No sunLight assigned.");
            return;
        }

        // Ray starts above Baune and goes in opposite direction of sunlight
        Vector3 rayOrigin = transform.position;
        Vector3 rayDirection = -sunLight.transform.forward;

        if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, checkDistance, coverLayer))
        {
            isInShade = true;
        }
        else
        {
            isInShade = false;
        }

        // Debug ray (green = in shade, red = in sun)
        Debug.DrawRay(rayOrigin, rayDirection * checkDistance, isInShade ? Color.green : Color.red);
    }
}
