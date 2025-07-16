using UnityEngine;


public class SunlightDetector : MonoBehaviour

{
    [Header("Detection Settings")]
    public Light sunLight;             // Reference to your directional light
    public float checkDistance = 100f; // How far to check (long enough to reach from sun's direction)
    public LayerMask coverLayer;       // Only detect objects that can block the sun
private PlayerMovement playerMovement;

    [Header("Debug")]
    public bool isInShade = false;
    private bool wasInShade = false;

void Start()
{
    playerMovement = GetComponent<PlayerMovement>();
}

    void Update()
    {
        CheckForShade();
        if (isInShade && !wasInShade)
{
    AudioManager.Instance?.PlayShadeTone();
}
wasInShade = isInShade;

    }

void CheckForShade()
{
    if (sunLight == null)
    {
        Debug.LogWarning("SunlightDetector: No sunLight assigned.");
        return;
    }

    // Automatically in shade if in a bush
    if (playerMovement != null && playerMovement.IsInBush())
    {
        isInShade = true;
        Debug.DrawRay(transform.position, -sunLight.transform.forward * checkDistance, Color.cyan);
        return;
    }

    // Otherwise do a raycast check
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

    Debug.DrawRay(rayOrigin, rayDirection * checkDistance, isInShade ? Color.green : Color.red);
}

}
