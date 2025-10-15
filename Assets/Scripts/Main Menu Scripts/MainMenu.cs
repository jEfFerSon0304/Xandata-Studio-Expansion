using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public GameObject mainMenuPanel;
    public GameObject startGamePanel;
    public GameObject howToPlayPanel;
    public GameObject settingsPanel;
    

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
}
