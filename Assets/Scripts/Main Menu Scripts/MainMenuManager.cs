using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;
    public GameObject exitConfirmPanel;

    public void StartGame()
    {
        SceneManager.LoadScene("GameScene"); // Replace with your gameplay scene name
    }

    public void OpenHowToPlay()
    {
        SceneManager.LoadScene("HowToPlay"); // Replace with your how-to-play scene name
    }

    public void OpenSettings()
    {
        settingsPanel.SetActive(true);
        mainMenuPanel.SetActive(false);
    }

    public void OpenExitConfirm()
    {
        exitConfirmPanel.SetActive(true);
        mainMenuPanel.SetActive(false);
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    public void CloseExitConfirm()
    {
        exitConfirmPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    public void ExitGame()
    {
        Application.Quit();
        Debug.Log("Quit Game!"); // Only works in builds
    }
}
