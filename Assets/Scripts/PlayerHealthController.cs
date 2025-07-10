using UnityEngine;

public class PlayerHealthController : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float healthDrainRate = 10f;  // % per second in sun
    public float healthRecoverRate = 5f; // % per second in shade
    public float recoverableBuffer = 5f; // Max % above current health recoverable can reach

    [Header("References")]
    public SunlightDetector sunlightDetector;

    [Header("Debug Info")]
    public float currentHealth;
    public float recoverableHealth;

    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        recoverableHealth = maxHealth;
    }

    void Update()
    {
        if (isDead) return;

        if (sunlightDetector == null)
        {
            Debug.LogWarning("PlayerHealthController: No SunlightDetector assigned!");
            return;
        }

        if (sunlightDetector.isInShade)
        {
            RecoverHealth();
        }
        else
        {
            DrainHealth();
        }

        if (currentHealth <= 0f && !isDead)
        {
            isDead = true;
            Debug.Log("Player Died");
            // TODO: Hook death mechanics here later
        }
    }

    void DrainHealth()
    {
        float drainAmount = healthDrainRate * Time.deltaTime;
        currentHealth -= drainAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        // Adjust recoverable health ONLY if it's too high
        float maxRecoverable = Mathf.Min(currentHealth + recoverableBuffer, maxHealth);
        if (recoverableHealth > maxRecoverable)
        {
            recoverableHealth = Mathf.MoveTowards(recoverableHealth, maxRecoverable, drainAmount);
        }
    }


    void RecoverHealth()
    {
        if (currentHealth < recoverableHealth)
        {
            float recoverAmount = healthRecoverRate * Time.deltaTime;
            currentHealth += recoverAmount;
            currentHealth = Mathf.Min(currentHealth, recoverableHealth);
        }
    }

    // Call this from your Feast script
    public void ApplyFeastHeal(float healAmount = 10f)
    {
        currentHealth += healAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        recoverableHealth = currentHealth; // Fully healed, no buffer
    }
    
    public void GainHealth(float amount)
{
    currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
    recoverableHealth = Mathf.Max(recoverableHealth, currentHealth);
}

}
