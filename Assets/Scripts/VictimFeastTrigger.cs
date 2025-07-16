using UnityEngine;
using Unity.Cinemachine;

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

    private VictimFleeController fleeController;
    private GameObject spawnedRadius;
    private GameObject feastPromptInstance;

    private Transform playerTransform;
    private PlayerMovement playerMovement;
    private HeartbeatController heartbeatController;

    private bool hasBeenFeastedOn = false;

    private bool feastPromptWasVisible = false;


    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerTransform = other.transform;
        playerMovement = playerTransform.GetComponent<PlayerMovement>();
        heartbeatController = playerTransform.GetComponent<HeartbeatController>();
        fleeController = GetComponent<VictimFleeController>();

        // Spawn sneak circle
        if (sneakRadiusPrefab != null && spawnedRadius == null)
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

            // Trigger feast camera once
            FeastCameraController.Instance?.ActivateFeastCamera(transform);

            // Camera shake
            CinemachineImpulseSource impulse = GetComponent<CinemachineImpulseSource>();
            impulse?.GenerateImpulse();
        }

        // Spawn feast prompt
        if (feastPromptPrefab != null && feastPromptInstance == null)
        {
            feastPromptInstance = Instantiate(
                feastPromptPrefab,
                transform.position + Vector3.up * 2f,
                Quaternion.identity,
                transform
            );

            feastPromptInstance.SetActive(false);
        }
    }

    void Update()
    {
        if (playerTransform == null || playerMovement == null || feastPromptInstance == null)
            return;

        float dist = Vector3.Distance(playerTransform.position, transform.position);
        bool isSneaking = playerMovement.IsSneaking();
        bool isStationary = playerMovement.IsStandingStill();
        bool isPreySafe = fleeController != null && fleeController.IsInCooldown;

        bool isFeastable = dist <= feastActivationRange &&
                           (isSneaking || isStationary) &&
                           !isPreySafe;


       // Detect transition into feastable state
if (isFeastable && !feastPromptWasVisible)
{
    feastPromptInstance.SetActive(true);
    feastPromptWasVisible = true;

    // 🎻 Play violin on feast readiness
    AudioManager.Instance?.PlayFeastViolin();
}
else if (!isFeastable && feastPromptWasVisible)
{
    feastPromptInstance.SetActive(false);
    feastPromptWasVisible = false;
}


        // Player initiates feast
        if (isFeastable && playerMovement.IsFeastPressed() && !hasBeenFeastedOn)
        {
            Debug.Log("FEAST ACTIVATED!");

            feastEffectController.TriggerFeastEffect();
            FeastometerController.Instance?.AddFeast(0.2f);

            PlayerHealthController health = playerTransform.GetComponent<PlayerHealthController>();
            health?.GainHealth(health.maxHealth * 0.10f);

            hasBeenFeastedOn = true;

            if (spawnedRadius) Destroy(spawnedRadius);
            if (feastPromptInstance) Destroy(feastPromptInstance);

            FeastCameraController.Instance?.ResetToDefault();
            Destroy(gameObject, 1f);
        }

        // Player startles the prey
        if (dist <= feastActivationRange &&
            !isSneaking && !isStationary &&
            !hasBeenFeastedOn && !isPreySafe)
        {
            Debug.Log("Player startled the prey!");

            fleeController?.StartFlee(playerTransform.position, () =>
            {
                if (spawnedRadius) Destroy(spawnedRadius);
                if (feastPromptInstance) Destroy(feastPromptInstance);
                FeastCameraController.Instance?.ResetToDefault();
            });
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        heartbeatController?.ResetHeartbeat();

        playerTransform = null;
        playerMovement = null;

        if (spawnedRadius)
        {
            Destroy(spawnedRadius);
            FeastCameraController.Instance?.ResetToDefault();
        }

        if (feastPromptInstance)
        {
            Destroy(feastPromptInstance);
        }
    }
}
