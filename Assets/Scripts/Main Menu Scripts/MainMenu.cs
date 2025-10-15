using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public GameObject mainMenuPanel;
    public GameObject startGamePanel;
    public GameObject howToPlayPanel;
    public GameObject settingsPanel;

    void Start()
    {
        // Only the main menu is active at start
        mainMenuPanel.SetActive(true);
        startGamePanel.SetActive(false);
        howToPlayPanel.SetActive(false);
        settingsPanel.SetActive(false);
    }


    public void OpenPanel(GameObject panelToOpen)
    {
        // Close all panels first
        mainMenuPanel.SetActive(false);
        startGamePanel.SetActive(false);
        howToPlayPanel.SetActive(false);
        settingsPanel.SetActive(false);
        

        // Open the selected one
        panelToOpen.SetActive(true);
    }

    public void BackToMainMenu()
    {
        // Close all panels first
        startGamePanel.SetActive(false);
        howToPlayPanel.SetActive(false);
        settingsPanel.SetActive(false);

        // Open the main menu
        mainMenuPanel.SetActive(true);
    }

}
