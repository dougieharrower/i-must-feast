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

    public float countdownTime = 120f; // 2 minutes
    private float currentTime;
    private bool isPaused = false;
    private bool gameEnded = false;

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
        if (feastedCount >= totalPrey)
            TriggerGameOver();
    }

    public void TriggerGameOver()
    {
        gameEnded = true;
        Debug.Log("🏁 Game Over");
        SceneManager.LoadScene("MenuScene"); // or whatever you want to call it
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
