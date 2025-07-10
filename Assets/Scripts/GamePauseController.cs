using UnityEngine;
using UnityEngine.InputSystem; // ✅ New Input System

public class GamePauseController : MonoBehaviour
{
    public GameObject pauseOverlay;

    private bool isPaused = false;

    void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame || Gamepad.current?.startButton.wasPressedThisFrame == true)
        {
            TogglePause();
        }
    }

    void TogglePause()
    {
        isPaused = !isPaused;
        pauseOverlay.SetActive(isPaused);
        Time.timeScale = isPaused ? 0f : 1f;
    }
}
