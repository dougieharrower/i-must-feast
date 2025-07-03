using UnityEngine;

public class VictimFeastTrigger : MonoBehaviour
{
    [Header("Visual Radius Settings")]
    public GameObject sneakRadiusPrefab;
    public float sneakRadiusVisualSize = 4f;

    [Header("Feast Prompt")]
    public GameObject feastPromptPrefab;
    public float feastActivationRange = 1.5f;

    [Header("Feast Effect Overlay")]
public FeastEffectController feastEffectController;


    private GameObject spawnedRadius;
    private GameObject feastPromptInstance;
    private Transform playerTransform;
    private PlayerMovement playerMovement;
    private bool hasBeenFeastedOn = false;


    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerTransform = other.transform;
            playerMovement = playerTransform.GetComponent<PlayerMovement>();

            // Spawn circle
            if (spawnedRadius == null && sneakRadiusPrefab != null)
            {
                spawnedRadius = Instantiate(
                    sneakRadiusPrefab,
                    transform.position + Vector3.up * 0.01f,
                    Quaternion.identity
                );
                spawnedRadius.transform.localScale = new Vector3(
                    sneakRadiusVisualSize,
                    0.01f,
                    sneakRadiusVisualSize
                );
            }

            // Spawn floating FEAST prompt
            if (feastPromptPrefab != null && feastPromptInstance == null)
            {
                feastPromptInstance = Instantiate(
                    feastPromptPrefab,
                    transform.position + Vector3.up * 2f,
                    Quaternion.identity,
                    transform // optional: follow this object
                );
                feastPromptInstance.SetActive(false); // Only show it if conditions are met
            }
        }
    }

void Update()
{
    if (playerTransform == null || playerMovement == null || feastPromptInstance == null)
    {
        return;
    }

    float dist = Vector3.Distance(playerTransform.position, transform.position);
    bool isSneaking = playerMovement.IsSneaking();
    bool isStationary = playerMovement.IsStandingStill();

    if (dist <= feastActivationRange && (isSneaking || isStationary))
    {
        if (!feastPromptInstance.activeSelf)
        {
            Debug.Log("Showing feast prompt.");
        }
        feastPromptInstance.SetActive(true);

        // FEAST input check (new!)
        if (!hasBeenFeastedOn && playerMovement.IsFeastPressed())
        {
            Debug.Log("FEAST ACTIVATED!");
            feastEffectController.TriggerFeastEffect();
            hasBeenFeastedOn = true;

            // Remove all visuals immediately
            if (spawnedRadius != null)
            {
                Destroy(spawnedRadius);
            }
            if (feastPromptInstance != null)
            {
                Destroy(feastPromptInstance);
            }

            // Destroy this victim after short delay (optional: lets the effect play)
            Destroy(gameObject, 1f);
        }
    }
    else
    {
        if (feastPromptInstance.activeSelf)
        {
            Debug.Log("Hiding feast prompt.");
        }
        feastPromptInstance.SetActive(false);
    }
}

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerTransform = null;
            playerMovement = null;

            if (spawnedRadius != null)
                Destroy(spawnedRadius);

            if (feastPromptInstance != null)
                Destroy(feastPromptInstance);
        }
    }
}
