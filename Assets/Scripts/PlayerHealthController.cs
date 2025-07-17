using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class PlayerHealthController : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float healthDrainRate = 10f;
    public float healthRecoverRate = 5f;
    public float recoverableBuffer = 5f;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI frenzyCountdownText;
    public GameObject deathScreen;

    [Header("References")]
    public SunlightDetector sunlightDetector;

    [Header("Debug Info")]
    public float currentHealth;
    public float recoverableHealth;

    public Material[] burnMaterials;

    private bool isDead = false;
    private bool isInFrenzy = false;
    private float frenzyTimer = 0f;
    private const float frenzyDuration = 5f;

    void Start()
    {
        currentHealth = maxHealth;
        recoverableHealth = maxHealth;
    }

    void Update()
    {
        foreach (Material mat in burnMaterials)
        {
            mat.SetFloat("_HealthPrecent", currentHealth / maxHealth);
        }

        if (isDead) return;

        if (sunlightDetector == null)
        {
            Debug.LogWarning("PlayerHealthController: No SunlightDetector assigned!");
            return;
        }

        if (isInFrenzy)
        {
            HandleFrenzy();
        }
        else
        {
            if (sunlightDetector.isInShade)
            {
                RecoverHealth();
            }
            else
            {
                DrainHealth();
            }

            if (currentHealth <= 0f && recoverableHealth > 0f && !isInFrenzy)
            {
                StartFrenzy();
            }
        }

        if (currentHealth <= 0f && recoverableHealth <= 0f && !isDead)
        {
            HandleDeath();
        }
    }

    void StartFrenzy()
    {
        isInFrenzy = true;
        frenzyTimer = frenzyDuration;
        Debug.Log("FRENZY MODE ACTIVATED! Find shade or feast!");

        if (frenzyCountdownText != null)
        {
            frenzyCountdownText.gameObject.SetActive(true);
            frenzyCountdownText.text = $"FRENZY: {Mathf.CeilToInt(frenzyTimer)}";
        }
    }

    void HandleFrenzy()
    {
        frenzyTimer -= Time.deltaTime;

        if (frenzyCountdownText != null)
        {
            frenzyCountdownText.text = $"FRENZY: {Mathf.CeilToInt(frenzyTimer)}";
        }

        if (currentHealth > 0f)
        {
            EndFrenzy();
            return;
        }

        if (frenzyTimer <= 0f)
        {
            if (sunlightDetector.isInShade)
            {
                Debug.Log("Player barely survived Frenzy by reaching shade!");
                currentHealth = Mathf.Max(1f, recoverableHealth);
                EndFrenzy();
            }
            else
            {
                Debug.Log("Frenzy expired. Player died.");
                currentHealth = 0f;
                recoverableHealth = 0f;
                isDead = true;
                EndFrenzy();
                HandleDeath();
            }
        }
    }

    void EndFrenzy()
    {
        isInFrenzy = false;

        if (frenzyCountdownText != null)
        {
            frenzyCountdownText.gameObject.SetActive(false);
        }

        Debug.Log("Frenzy ended.");
    }

    void HandleDeath()
    {
        isDead = true;
        Debug.Log("Player has died.");
        if (deathScreen != null)
        {
            deathScreen.SetActive(true);
        }
        FeastGameManager.Instance?.TriggerGameOver();
    }

    void DrainHealth()
    {
        float drainAmount = healthDrainRate * Time.deltaTime;
        currentHealth -= drainAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

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

    public void ApplyFeastHeal(float healAmount = 10f)
    {
        currentHealth += healAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        recoverableHealth = currentHealth;
    }

    public void GainHealth(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        recoverableHealth = Mathf.Max(recoverableHealth, currentHealth);
    }

    public void TakeDamage(float damageAmount)
    {
        if (isInFrenzy)
        {
            recoverableHealth -= damageAmount;
            recoverableHealth = Mathf.Clamp(recoverableHealth, 0f, maxHealth);
            Debug.Log($"Frenzy Damage! Recoverable Health: {recoverableHealth}");

            if (recoverableHealth <= 0f)
            {
                Debug.Log("Player killed during Frenzy!");
                currentHealth = 0f;
                isDead = true;
                HandleDeath();
            }
        }
        else
        {
            currentHealth -= damageAmount;
            currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
            Debug.Log($"Baune took {damageAmount} damage! Current health: {currentHealth}");
        }
    }

    public class CharacterBurnEffect : MonoBehaviour
    {
        public PlayerHealthController playerHealth;
        public Renderer[] affectedRenderers;
        private static readonly int HealthPrecentID = Shader.PropertyToID("_HealthPrecent");

        void Update()
        {
            float healthPercent = Mathf.Clamp01(playerHealth.currentHealth / playerHealth.maxHealth);
            foreach (Renderer rend in affectedRenderers)
            {
                foreach (Material mat in rend.materials)
                {
                    if (mat.HasProperty(HealthPrecentID))
                    {
                        mat.SetFloat(HealthPrecentID, healthPercent);
                    }
                }
            }
        }
    }
}
