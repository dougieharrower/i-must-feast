using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VictimFleeController : MonoBehaviour
{
    [Header("Flee Settings")]
    public float moveSpeed = 3f;
    public float fleeDistance = 15f;
    public float cooldownTime = 5f;
    public int numTurns = 2;
    public float turnAngleMin = 30f;
    public float turnAngleMax = 60f;

    [Header("Ground Settings")]
    public LayerMask groundLayer;
    public float groundCheckHeight = 50f;

    [Header("Optional Bounds Fallback (Failsafe)")]
    public bool enableBoundsClamp = true;
    public float minX = -20f;
    public float maxX = 20f;
    public float minZ = -20f;
    public float maxZ = 20f;

    [Header("Perimeter Fallback Settings")]
    public bool enablePerimeterFallback = true;
    public float perimeterRadiusFactor = 0.9f;  // 90% of max circle
    public float minPerimeterAngle = 140f;
    public float maxPerimeterAngle = 210f;

    private bool isFleeing = false;
    private bool isInCooldown = false;

    public bool IsInCooldown => isInCooldown;

    public void StartFlee(Vector3 playerPosition, System.Action onFleeStart = null)
    {
        if (isFleeing || isInCooldown)
            return;

        bool fleeStarted = TryStartMeanderingFlee(playerPosition, onFleeStart);

        if (!fleeStarted && enablePerimeterFallback)
        {
            Debug.LogWarning("Primary flee failed; using perimeter fallback.");
            Vector3 fallbackTarget = FindPerimeterFleeTarget();
            isFleeing = true;
            onFleeStart?.Invoke();
            StartCoroutine(FleeRoutine(new List<Vector3> { fallbackTarget }));
        }
    }

    private bool TryStartMeanderingFlee(Vector3 playerPosition, System.Action onFleeStart)
    {
        Vector3 fleeDirection = transform.position - playerPosition;
        fleeDirection.y = 0f;
        fleeDirection = fleeDirection.normalized;

        List<Vector3> waypoints = new List<Vector3>();
        Vector3 currentPos = transform.position;

        Vector3? firstPoint = SnapToGround(currentPos + fleeDirection * (fleeDistance * 0.5f));
        if (firstPoint.HasValue)
        {
            waypoints.Add(firstPoint.Value);
        }
        else
        {
            Debug.LogWarning("Failed to find valid first flee point; canceling meandering flee.");
            return false;
        }

        Vector3 lastDirection = fleeDirection;

        for (int i = 0; i < numTurns; i++)
        {
            float angle = Random.Range(-turnAngleMax, turnAngleMax);
            Vector3 turnDir = Quaternion.Euler(0f, angle, 0f) * lastDirection;
            Vector3? nextPoint = SnapToGround(waypoints[waypoints.Count - 1] + turnDir * (fleeDistance * 0.25f));

            if (nextPoint.HasValue)
            {
                waypoints.Add(nextPoint.Value);
                lastDirection = turnDir;
            }
            else
            {
                Debug.LogWarning($"Failed to find valid waypoint for turn {i + 1}, stopping early.");
                break;
            }
        }

        isFleeing = true;
        onFleeStart?.Invoke();
        StartCoroutine(FleeRoutine(waypoints));
        return true;
    }

    private Vector3 FindPerimeterFleeTarget()
    {
        Vector3 currentPos = transform.position;

        float distLeft = currentPos.x - minX;
        float distRight = maxX - currentPos.x;
        float distBack = currentPos.z - minZ;
        float distForward = maxZ - currentPos.z;

        float maxRadius = Mathf.Min(distLeft, distRight, distBack, distForward) * perimeterRadiusFactor;

        float startAngle = Random.Range(0f, 360f);
        float offsetAngle = Random.Range(minPerimeterAngle, maxPerimeterAngle);

        float targetAngle = startAngle + offsetAngle;

        float targetX = currentPos.x + maxRadius * Mathf.Cos(targetAngle * Mathf.Deg2Rad);
        float targetZ = currentPos.z + maxRadius * Mathf.Sin(targetAngle * Mathf.Deg2Rad);

        Vector3 targetPos = new Vector3(targetX, transform.position.y, targetZ);

        Debug.Log($"Perimeter Fallback Target: {targetPos}");
        return targetPos;
    }

    private Vector3? SnapToGround(Vector3 position)
    {
        Vector3 rayStart = new Vector3(position.x, groundCheckHeight, position.z);
        if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, Mathf.Infinity, groundLayer))
        {
            return hit.point;
        }
        else if (enableBoundsClamp)
        {
            Debug.LogWarning("Prey target out of bounds, clamping as fallback.");
            position.x = Mathf.Clamp(position.x, minX, maxX);
            position.z = Mathf.Clamp(position.z, minZ, maxZ);
            position.y = transform.position.y;
            return position;
        }
        else
        {
            Debug.LogWarning("Prey couldn't find ground below target point!");
            return null;
        }
    }

    private IEnumerator FleeRoutine(List<Vector3> waypoints)
    {
        foreach (Vector3 target in waypoints)
        {
            while (Vector3.Distance(new Vector3(transform.position.x, 0f, transform.position.z),
                                   new Vector3(target.x, 0f, target.z)) > 0.1f)
            {
                Vector3 direction = target - transform.position;
                direction.y = 0f;
                direction = direction.normalized;

                transform.position += direction * moveSpeed * Time.deltaTime;

                if (enableBoundsClamp)
                {
                    Vector3 clampedPosition = transform.position;
                    clampedPosition.x = Mathf.Clamp(clampedPosition.x, minX, maxX);
                    clampedPosition.z = Mathf.Clamp(clampedPosition.z, minZ, maxZ);
                    transform.position = clampedPosition;
                }

                if (direction != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
                }

                yield return null;
            }
        }

        isFleeing = false;
        StartCoroutine(CooldownRoutine());
    }

    private IEnumerator CooldownRoutine()
    {
        isInCooldown = true;
        yield return new WaitForSeconds(cooldownTime);
        isInCooldown = false;
    }
}
