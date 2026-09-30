using UnityEngine;
using UnityEngine.InputSystem; // ✅ New Input System

public class GamePauseController : MonoBehaviour
{
    public GameObject pauseOverlay;

    private bool isPaused = false;

    void Update()
    {
        bool keyboardPause = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
        bool gamepadPause = Gamepad.current?.startButton.wasPressedThisFrame == true;

        if (keyboardPause || gamepadPause)
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
