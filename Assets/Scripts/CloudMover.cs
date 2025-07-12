using UnityEngine;

public class CloudMover : MonoBehaviour
{
    private Vector3 direction;
    private float speed;
    private float destroyDistance;
    private Vector3 origin;

    private bool initialized = false;

    public void Initialize(Vector3 moveDirection, Vector2 speedRange, float destroyThreshold)
    {
        direction = moveDirection.normalized;
        speed = Random.Range(speedRange.x, speedRange.y);
        destroyDistance = destroyThreshold;
        origin = transform.position;
        initialized = true;
    }

    void Update()
    {
        if (!initialized) return;

        transform.position += direction * speed * Time.deltaTime;

        if (Vector3.Distance(origin, transform.position) > destroyDistance)
            Destroy(gameObject);
    }
}
