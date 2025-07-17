using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.InputSystem;

public class FeastCameraController : MonoBehaviour
{
    public static FeastCameraController Instance;

    [Header("Cinemachine Cameras")]
    public CinemachineCamera defaultCamera;
    public CinemachineCamera feastCamera;

    [Header("Feast Camera Toggle")]
    public bool feastCamEnabled = true;

    [Header("Feast Camera Zoom")]
    public float feastFOV = 30f;
    private float defaultFOV;

    [Header("Obstruction Check")]
    public LayerMask obstructionMask;

    private Transform currentPrey;
    private Transform playerTransform;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Update()
    {
        if (Keyboard.current.fKey.wasPressedThisFrame && currentPrey != null)
        {
            ActivateFeastCamera(currentPrey);
        }

        if (Keyboard.current.gKey.wasPressedThisFrame)
        {
            ResetToDefault();
        }
    }

    public void ActivateFeastCamera(Transform victim)
    {
        if (!feastCamEnabled)
        {
            Debug.Log("📷 FeastCam disabled in inspector.");
            return;
        }

        if (feastCamera == null || defaultCamera == null || victim == null)
            return;

        currentPrey = victim;

        // Get vector from victim to player
        Vector3 toPlayer = (playerTransform.position - victim.position).normalized;

        // Proposed camera position
        Vector3 cameraOffset = -toPlayer * 6f + Vector3.up * 1f;
        Vector3 proposedCameraPos = victim.position + cameraOffset;

        // Check for obstruction
        bool hitPrey = Physics.Linecast(proposedCameraPos, victim.position, out _, obstructionMask);
        bool hitPlayer = Physics.Linecast(proposedCameraPos, playerTransform.position, out _, obstructionMask);

        if (hitPrey || hitPlayer)
        {
            Debug.Log("❌ FeastCam blocked. Staying on default.");
            return;
        }

        // Set position + look
        feastCamera.transform.position = proposedCameraPos;
        Vector3 midpoint = Vector3.Lerp(victim.position, playerTransform.position, 0.5f);
        feastCamera.LookAt = null;
        feastCamera.transform.LookAt(midpoint + Vector3.up * 0.5f);

        defaultFOV = feastCamera.Lens.FieldOfView;
        feastCamera.Lens.FieldOfView = feastFOV;

        feastCamera.Priority = 100;
        defaultCamera.Priority = 10;

        Debug.Log("✅ Feast camera activated.");
    }

    public void ResetToDefault()
    {
        if (feastCamera == null || defaultCamera == null)
            return;

        feastCamera.Priority = 0;
        defaultCamera.Priority = 100;

        feastCamera.Lens.FieldOfView = defaultFOV;
        feastCamera.LookAt = null;
        currentPrey = null;

        Debug.Log("↩️ Camera reset to default.");
    }
}
