using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FeastEffectController : MonoBehaviour
{
    public Image flashImage;
    public TextMeshProUGUI feastText;
    public float flashDuration = 0.5f;

    private void Awake()
    {
        HideOverlay();
    }

    public void TriggerFeastEffect()
    {
        StopAllCoroutines();
        StartCoroutine(FlashRoutine());
    }

    private System.Collections.IEnumerator FlashRoutine()
    {
        float elapsed = 0f;

        // Fade in
        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / flashDuration;

            flashImage.color = new Color(1f, 0f, 0f, Mathf.Lerp(0f, 1f, t));  // Red fade-in
            feastText.color = new Color(1f, 1f, 1f, Mathf.Lerp(0f, 1f, t));    // Text fade-in
            yield return null;
        }

        // Hold briefly
        yield return new WaitForSeconds(0.5f);

        // Fade out
        elapsed = 0f;
        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / flashDuration;

            flashImage.color = new Color(1f, 0f, 0f, Mathf.Lerp(1f, 0f, t));
            feastText.color = new Color(1f, 1f, 1f, Mathf.Lerp(1f, 0f, t));
            yield return null;
        }

        HideOverlay();
    }

    private void HideOverlay()
    {
        flashImage.color = new Color(1f, 0f, 0f, 0f);
        feastText.color = new Color(1f, 1f, 1f, 0f);
    }
}
