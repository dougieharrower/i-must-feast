using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIHealthBarController : MonoBehaviour
{
    public PlayerHealthController playerHealth;
    public Image healthBar;
    public Image recoverableBar;
    public TextMeshProUGUI healthText;

    [Header("Color Settings")]
    public Color healthyColor = Color.green;
    public Color midColor = new Color(1f, 0.65f, 0f); // Orange
    public Color lowColor = Color.red;

    void Update()
    {
        if (playerHealth == null) return;

        float healthPercent = playerHealth.currentHealth / playerHealth.maxHealth;
        float recoverablePercent = playerHealth.recoverableHealth / playerHealth.maxHealth;

        healthBar.fillAmount = healthPercent;
        recoverableBar.fillAmount = recoverablePercent;

        // Smooth color transition
        Color targetColor;
        if (healthPercent > 0.5f)
        {
            float t = Mathf.InverseLerp(0.5f, 1f, healthPercent);
            targetColor = Color.Lerp(midColor, healthyColor, t);
        }
        else
        {
            float t = Mathf.InverseLerp(0f, 0.5f, healthPercent);
            targetColor = Color.Lerp(lowColor, midColor, t);
        }
        healthBar.color = targetColor;

        // Update % text (round nicely)
        int displayedPercent = Mathf.RoundToInt(healthPercent * 100f);
        healthText.text = displayedPercent + "%";
    }
}
