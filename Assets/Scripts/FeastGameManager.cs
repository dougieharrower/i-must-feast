using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class FeastGameManager : MonoBehaviour
{
    public static FeastGameManager Instance;

    [Header("UI")]
    public TextMeshProUGUI preyCounterText;
    public TextMeshProUGUI timerText;
    public Image preyIconImage;

    private int totalPrey;
    private int feastedCount = 0;
    private float currentTime;

    public float countdownTime = 120f; // 2 minutes

    private bool isPaused = false;
    private bool gameEnded = false;

    // 🟨 NEW: Score & Feral tracking
    private int score = 0;
    private bool usedFeralMode = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        totalPrey = VictimSpawner.Instance.TotalVictims;
        currentTime = countdownTime;
        UpdateUI();
    }

    void Update()
    {
        if (isPaused || gameEnded)
            return;

        currentTime -= Time.deltaTime;
        if (currentTime <= 0f)
        {
            currentTime = 0f;
            TriggerGameOver();
        }

        UpdateUI();
    }

    public void RegisterFeast()
    {
        feastedCount++;
        score += 100;

        if (feastedCount >= totalPrey)
            TriggerGameOver();
    }

    // 🟨 NEW: Called from FeralModeManager
    public void RegisterFeralUsage()
    {
        usedFeralMode = true;
    }

    private int GetFinalScore()
    {
        int timeBonus = Mathf.RoundToInt(currentTime) * 10;
        int feralBonus = usedFeralMode ? 50 : 0;
        return score + timeBonus + feralBonus;
    }

    public void TriggerGameOver()
    {
        if (gameEnded) return;

        gameEnded = true;
        Debug.Log("🏁 Game Over");

        // 🟨 NEW: Store results
        GameResultStore.FinalScore = GetFinalScore();
        GameResultStore.PreyCount = feastedCount;
        GameResultStore.TotalPrey = totalPrey;
        GameResultStore.TimeLeft = currentTime;
        GameResultStore.FeralUsed = usedFeralMode;

        // Load results screen
        SceneManager.LoadScene("ResultsScene");
    }

    private void UpdateUI()
    {
        if (preyCounterText != null)
            preyCounterText.text = $"{feastedCount} / {totalPrey}";

        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(currentTime / 60f);
            int seconds = Mathf.FloorToInt(currentTime % 60f);
            timerText.text = $"{minutes:00}:{seconds:00}";
        }
    }

    public void SetPaused(bool paused)
    {
        isPaused = paused;
    }
}
