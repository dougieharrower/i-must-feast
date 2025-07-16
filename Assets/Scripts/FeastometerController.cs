using UnityEngine;
using UnityEngine.UI;

public class FeastometerController : MonoBehaviour
{
    public static FeastometerController Instance { get; private set; }

    [Header("Feastometer UI")]
    public Image feastBarFill;

    [Header("Settings")]
    public float decayRate = 0.001f; // 1% every 10 seconds
    public float fillSurgeSpeed = 3f;
    public float feralThreshold = 1f;

    private float targetFill = 0f;
    private float currentFill = 0f;

    private bool isFeralTriggered = false;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this.gameObject);
        else Instance = this;
    }

    void Update()
    {
        if (isFeralTriggered) return;

        // Smooth surge fill
        currentFill = Mathf.MoveTowards(currentFill, targetFill, fillSurgeSpeed * Time.deltaTime);
        feastBarFill.fillAmount = currentFill;

        // Passive decay
        if (targetFill > 0f)
        {
            targetFill -= decayRate * Time.deltaTime;
            targetFill = Mathf.Clamp01(targetFill);
        }

        // Trigger Feral Mode
        if (currentFill >= feralThreshold)
        {
            TriggerFeralMode();
        }
    }

    public void AddFeast(float amount)
    {
        if (isFeralTriggered) return;

        targetFill += amount;
        targetFill = Mathf.Clamp01(targetFill);
        Debug.Log($"Feast added: {amount}. Total: {targetFill}");
    }

    void TriggerFeralMode()
    {
        Debug.Log("🎯 FERAL MODE ACTIVATED!");
        isFeralTriggered = true;
        FeralModeManager.Instance.StartFeralMode();

        // Reset bar after activation
        currentFill = 0f;
        targetFill = 0f;
        feastBarFill.fillAmount = 0f;

        // Delay reset until Feral ends
        Invoke(nameof(ResetFeralTrigger), FeralModeManager.Instance.feralDuration);
    }

    void ResetFeralTrigger()
    {
        isFeralTriggered = false;
    }
}
