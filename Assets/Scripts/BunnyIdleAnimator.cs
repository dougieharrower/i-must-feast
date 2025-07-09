using UnityEngine;
using System.Collections;

public class BunnyIdleAnimator : MonoBehaviour
{
    public Animator animator;
    public float minDelay = 3f;  // Minimum wait time between animations
    public float maxDelay = 8f;  // Maximum wait time between animations

    void Start()
    {
        // Auto-find Animator if not assigned
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        StartCoroutine(RandomIdleRoutine());
    }

    IEnumerator RandomIdleRoutine()
    {
        while (true)
        {
            // Wait a random amount of time between min and max delay
            float waitTime = Random.Range(minDelay, maxDelay);
            yield return new WaitForSeconds(waitTime);

            // Randomly pick either Graze or Survey
            int action = Random.Range(0, 2);
            if (action == 0)
            {
                animator.SetTrigger("Graze");
            }
            else
            {
                animator.SetTrigger("Survey");
            }
        }
    }
}
