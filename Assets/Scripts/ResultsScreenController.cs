using UnityEngine;
using TMPro;

public class ResultsScreenController : MonoBehaviour
{
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI preyText;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI feralText;

    void Start()
    {
        finalScoreText.text = $"Score: {GameResultStore.FinalScore}";
        preyText.text = $"Prey Feasted: {GameResultStore.PreyCount} / {GameResultStore.TotalPrey}";

        int minutes = Mathf.FloorToInt(GameResultStore.TimeLeft / 60f);
        int seconds = Mathf.FloorToInt(GameResultStore.TimeLeft % 60f);
        timeText.text = $"Time Left: {minutes:00}:{seconds:00}";

        feralText.text = GameResultStore.FeralUsed ? "Feral Mode Activated!" : "No Feral Mode Used";
    }
}
