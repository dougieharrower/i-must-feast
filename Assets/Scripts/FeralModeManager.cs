using UnityEngine;
using System.Collections;

public class FeralModeManager : MonoBehaviour
{
    public static FeralModeManager Instance { get; private set; }

    [Header("Feral Settings")]
    public float feralDuration = 20f;
    public bool noSunlightDamage = true;
    public bool noGnomeDamage = true;
    public bool instantFeast = true;
    public bool superSpeed = true;
    public bool timeSlow = false;

    public bool IsActive { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this.gameObject);
        else Instance = this;
    }

    public void StartFeralMode()
    {
        if (IsActive) return;

        IsActive = true;
        StartCoroutine(FeralRoutine());

        // Optional: apply modifiers here (like slow time)
        if (timeSlow)
        {
            Time.timeScale = 0.5f;
        }

        Debug.Log("🐾 Feral Mode started!");
    }

    private IEnumerator FeralRoutine()
    {
        float timer = feralDuration;

        while (timer > 0f)
        {
            timer -= Time.unscaledDeltaTime;
            yield return null;
        }

        EndFeralMode();
    }

    void EndFeralMode()
    {
        IsActive = false;

        if (timeSlow)
        {
            Time.timeScale = 1f;
        }

        Debug.Log("🛑 Feral Mode ended.");
    }
}
