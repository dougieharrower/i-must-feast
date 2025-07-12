using UnityEngine;
using UnityEngine.AI;

public class GnomeThreat : MonoBehaviour
{
    enum GnomeState { Patrol, Chase, Cooldown }
    GnomeState currentState = GnomeState.Patrol;

    NavMeshAgent agent;
    Transform baune;
    float cooldownTimer = 0f;

    [Header("Detection Settings")]
    public float detectionRadius = 5f;
    public LayerMask shelterMask;

    [Header("Movement Speeds")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 6f;

    private PlayerMovement bauneMovement;


    [Header("References")]
    [SerializeField] private GameObject gnomeVisual; // Optional if needed later.
    [SerializeField] private GameObject smokeEffectPrefab; // Assign your smoke prefab here (optional, future use).

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        baune = GameObject.FindGameObjectWithTag("Player").transform;
        bauneMovement = baune.GetComponent<PlayerMovement>();

        PickPatrolPoint();
        
    }

    void Update()
    {
        switch (currentState)
        {
            case GnomeState.Patrol:
                PatrolBehavior();
                DetectBaune();
                break;

            case GnomeState.Chase:
                ChaseBehavior();
                break;

            case GnomeState.Cooldown:
                CooldownBehavior();
                break;
        }
    }

    void PatrolBehavior()
    {
        agent.speed = patrolSpeed;
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            Invoke(nameof(PickPatrolPoint), 2f);
        }
    }

    void PickPatrolPoint()
    {
        Vector3 randomPoint = RandomNavmeshLocation(10f);
        agent.SetDestination(randomPoint);
    }

void DetectBaune()
{
    Vector3 dirToBaune = (baune.position - transform.position);
    float distance = dirToBaune.magnitude;

    if (distance < detectionRadius)
    {
        // Line of sight check
        Ray ray = new Ray(transform.position + Vector3.up * 1.5f, dirToBaune.normalized);
        if (Physics.Raycast(ray, out RaycastHit hit, detectionRadius))
        {
            if (hit.collider.CompareTag("Player"))
            {
                // Player is directly visible
                bool inBush = bauneMovement != null && bauneMovement.IsInBush();
                bool isSneaking = bauneMovement != null && bauneMovement.IsSneaking();

                // Can't see sneaking Baune in a bush
                if (inBush && isSneaking)
                    return;

                // Otherwise, chase
                currentState = GnomeState.Chase;
            }
            else
            {
                // Something blocked the view (e.g., a bush or other shelter object)
                return;
            }
        }
    }
}


    void ChaseBehavior()
    {
        agent.speed = chaseSpeed;
        agent.SetDestination(baune.position);
    }

    void CooldownBehavior()
    {
        cooldownTimer -= Time.deltaTime;
        if (cooldownTimer <= 0f)
        {
            currentState = GnomeState.Patrol;
            PickPatrolPoint();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (currentState == GnomeState.Chase && other.CompareTag("Player"))
        {
            PlayerHealthController health = other.GetComponent<PlayerHealthController>();
            if (health != null)
            {
                health.TakeDamage(10f);
            }

            // Future smoke effect (optional)
            if (smokeEffectPrefab != null)
            {
                Instantiate(smokeEffectPrefab, transform.position, Quaternion.identity);
            }

            TeleportFarFromBaune();

            // Switch to cooldown before resuming patrol
            currentState = GnomeState.Cooldown;
            cooldownTimer = 2f; // Cooldown before resuming patrol.
        }
    }

    void TeleportFarFromBaune()
    {
        Vector3 bestPoint = transform.position;
        float farthestDistance = 0f;

        for (int i = 0; i < 50; i++)
        {
            Vector3 randomDirection = Random.insideUnitSphere * 50f;
            randomDirection.y = 0;
            Vector3 candidatePoint = baune.position + randomDirection;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(candidatePoint, out hit, 10f, NavMesh.AllAreas))
            {
                float dist = Vector3.Distance(hit.position, baune.position);
                if (dist > farthestDistance)
                {
                    farthestDistance = dist;
                    bestPoint = hit.position;
                }
            }
        }

        transform.position = bestPoint;
        Debug.Log($"Gnome teleported to {bestPoint} (distance {farthestDistance} from Baune)");
        agent.ResetPath();
    }

    Vector3 RandomNavmeshLocation(float radius)
    {
        Vector3 randomDirection = Random.insideUnitSphere * radius;
        randomDirection.y = 0;
        randomDirection += transform.position;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, radius, NavMesh.AllAreas))
        {
            return hit.position;
        }
        else
        {
            return transform.position;
        }
    }
}
