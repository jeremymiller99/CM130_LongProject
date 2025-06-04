using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    // Make sure these scene names match exactly what you've named them in Build Settings
    

    public void PlayGame()
    {
        SceneManager.LoadScene("IntroCutscene");
    }

    public void OpenSettings()
    {
        SceneManager.LoadScene("Settings");
    }

    public void ShowCredits()
    {
        SceneManager.LoadScene("Credits");
    }

    public void BackToMenu(){
        SceneManager.LoadScene("Menu");
    }

    public void QuitGame()
    {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
