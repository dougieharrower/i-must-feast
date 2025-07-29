using UnityEngine;
using UnityEngine.SceneManagement;

public class ResultsMenuActions : MonoBehaviour
{
public void RetryGame()
{
    // Optional: force reset singletons
    FeastGameManager.Instance = null;
    AudioManager.Instance = null;
    FeastCameraController.Instance = null;

    SceneManager.LoadScene("FeastPark");
}

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
