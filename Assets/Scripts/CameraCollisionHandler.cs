using UnityEngine;

public class CameraCollisionHandler : MonoBehaviour
{
    public Transform target; // usually the player
    public Transform cameraTransform; // the Cinemachine camera object
    public LayerMask collisionMask;
    public float defaultDistance = 5f;
    public float minDistance = 1f;
    public float cameraRadius = 0.2f;
    public float smoothSpeed = 10f;

    private Vector3 desiredPosition;

    void LateUpdate()
    {
        if (target == null || cameraTransform == null) return;

        Vector3 direction = (cameraTransform.position - target.position).normalized;
        float distance = defaultDistance;
        RaycastHit hit;

        // SphereCast from player to camera
        if (Physics.SphereCast(target.position, cameraRadius, direction, out hit, defaultDistance, collisionMask))
        {
            distance = Mathf.Clamp(hit.distance - cameraRadius, minDistance, defaultDistance);
        }

        desiredPosition = target.position + direction * distance;

        cameraTransform.position = Vector3.Lerp(cameraTransform.position, desiredPosition, Time.deltaTime * smoothSpeed);

        // Debug visualization
        Debug.DrawLine(target.position, cameraTransform.position, Color.yellow);
        Debug.DrawRay(target.position, direction * distance, Color.red);
    }
}
