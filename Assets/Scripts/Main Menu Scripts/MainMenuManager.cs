using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    [Header("Panels / UI Elements")]
    public GameObject mainMenuPanel;      // Contains everything (background, clouds, logo, buttons)
    public GameObject logo;               // Logo inside MainMenuPanel
    public GameObject buttonGroup;        // Buttons inside MainMenuPanel
    public GameObject settingsPanel;      // Settings panel
    public GameObject exitConfirmPanel;   // Exit confirmation panel (optional, later)

    // Called when Start Game button is clicked
    public void StartGame()
    {
        SceneManager.LoadScene("StartGame"); // Replace with your gameplay scene name
    }

    // Called when How To Play button is clicked
    public void OpenHowToPlay()
    {
        SceneManager.LoadScene("HowToPlayScene"); // Replace with your scene name
    }

    // Called when Settings button is clicked
    public void OpenSettings()
    {
        if (logo != null) logo.SetActive(false);           // hide logo
        if (buttonGroup != null) buttonGroup.SetActive(false); // hide buttons
        if (settingsPanel != null) settingsPanel.SetActive(true); // show settings panel
    }

    // Called when Back button in Settings is clicked
    public void CloseSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false); // hide settings panel
        if (logo != null) logo.SetActive(true);                     // show logo again
        if (buttonGroup != null) buttonGroup.SetActive(true);       // show buttons again
    }

    // Exit modal functions (ready for later)
    public void OpenExitConfirm()
    {
        if (exitConfirmPanel != null) exitConfirmPanel.SetActive(true);
        if (logo != null) logo.SetActive(false);
        if (buttonGroup != null) buttonGroup.SetActive(false);
    }

    public void CloseExitConfirm()
    {
        if (exitConfirmPanel != null) exitConfirmPanel.SetActive(false);
        if (logo != null) logo.SetActive(true);
        if (buttonGroup != null) buttonGroup.SetActive(true);
    }

    public void ExitGame()
    {
        Application.Quit();
        Debug.Log("Quit Game!"); // Only works in build
    }
}
