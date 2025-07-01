using UnityEngine;

public class SunlightDetector : MonoBehaviour
{
    [Header("Detection Settings")]
    public float rayHeightAbovePlayer = 10f;
    public LayerMask coverLayer; // Assign your "Cover" objects to this in the Inspector

    [Header("Debug")]
    public bool isInShade = false;

    void Update()
    {
        CheckForShade();
    }

    void CheckForShade()
    {
        Vector3 rayOrigin = transform.position + Vector3.up * rayHeightAbovePlayer;
        Vector3 rayDirection = Vector3.down;
        float rayDistance = rayHeightAbovePlayer + 1f;

        // Raycast down to see if something is blocking the sun
        if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, rayDistance, coverLayer))
        {
            isInShade = true;
        }
        else
        {
            isInShade = false;
        }

        // Optional: visualize ray
        Debug.DrawRay(rayOrigin, rayDirection * rayDistance, isInShade ? Color.green : Color.red);
    }
}
