using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenu : MonoBehaviour
{
    public void StartGame()
    {
        // Load your main scene — replace with correct name
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

    public void ShowOptions()
    {
        // Placeholder for future options panel
        Debug.Log("Options menu not implemented.");
    }
}
