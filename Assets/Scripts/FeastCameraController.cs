using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.InputSystem;

public class FeastCameraController : MonoBehaviour
{
    public static FeastCameraController Instance;

    [Header("Cinemachine Cameras")]
    public CinemachineCamera defaultCamera;
    public CinemachineCamera feastCamera;

    private Transform currentPrey;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Update()
    {
        // Debug keys
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
        if (feastCamera == null || defaultCamera == null || victim == null)
            return;

        currentPrey = victim;

        // Position the camera in front of the prey
        Vector3 offset = -victim.forward * 2.5f + Vector3.up * 1.5f;
        feastCamera.transform.position = victim.position + offset;

        // This tells Cinemachine's rotation system what to look at
        feastCamera.LookAt = victim;

        // Switch cameras by setting priorities
        feastCamera.Priority = 100;
        defaultCamera.Priority = 10;

        Debug.Log("✅ Feast camera activated with LookAt set.");
    }

    public void ResetToDefault()
    {
        if (feastCamera == null || defaultCamera == null)
            return;

        feastCamera.Priority = 0;
        defaultCamera.Priority = 100;

        feastCamera.LookAt = null;
        currentPrey = null;

        Debug.Log("↩️ Camera reset to default.");
    }
}
