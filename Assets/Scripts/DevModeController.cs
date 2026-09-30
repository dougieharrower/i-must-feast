using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

// Playtesting aid: F1 toggles god mode + a movement speed boost so you can
// chase down every prey without dying from sunlight/gnome damage. F2 (while
// active) reloads the current scene for a quick retry. Self-installs via
// RuntimeInitializeOnLoadMethod, so it needs no manual scene setup and is
// present in every scene automatically.
public class DevModeController : MonoBehaviour
{
    public static bool IsActive { get; private set; }
    public static float SpeedMultiplier { get; private set; } = 2.5f;

    private static DevModeController instance;
    private TextMeshProUGUI indicatorText;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (instance != null) return;

        GameObject go = new GameObject("DevModeController");
        instance = go.AddComponent<DevModeController>();
    }
#endif

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        BuildIndicatorUI();
    }

    void BuildIndicatorUI()
    {
        GameObject canvasGO = new GameObject("DevModeCanvas");
        canvasGO.transform.SetParent(transform);

        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;
        canvasGO.AddComponent<CanvasScaler>();

        GameObject textGO = new GameObject("DevModeText");
        textGO.transform.SetParent(canvasGO.transform, false);

        indicatorText = textGO.AddComponent<TextMeshProUGUI>();
        indicatorText.text = "DEV MODE\nF1 off | F2 reload | F3 win";
        indicatorText.fontSize = 22;
        indicatorText.color = new Color(1f, 0.3f, 0.3f, 0.9f);
        indicatorText.alignment = TextAlignmentOptions.TopRight;

        RectTransform rt = textGO.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(1f, 1f);
        rt.anchoredPosition = new Vector2(-20f, -20f);
        rt.sizeDelta = new Vector2(320f, 60f);

        indicatorText.gameObject.SetActive(IsActive);
    }

    void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.f1Key.wasPressedThisFrame)
        {
            IsActive = !IsActive;

            if (indicatorText != null)
                indicatorText.gameObject.SetActive(IsActive);

            Debug.Log(IsActive
                ? "DEV MODE ENABLED — god mode + speed boost active (F1 off, F2 reload scene, F3 instant win)"
                : "Dev mode disabled");
        }

        if (!IsActive) return;

        if (Keyboard.current.f2Key.wasPressedThisFrame)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        if (Keyboard.current.f3Key.wasPressedThisFrame)
        {
            FeastGameManager.Instance?.DevCompleteAll();
        }
    }
}
