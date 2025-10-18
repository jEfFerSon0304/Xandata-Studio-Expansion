using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class MainMenuController : MonoBehaviour
{
    public GameObject hostJoinPanel;

    void Start()
    {
        hostJoinPanel.SetActive(false);
    }

    public void StartGame()
    {
        hostJoinPanel.SetActive(true);
    }

    public void HostGame()
    {
        LobbyNetworkStarter.startAsHost = true;
        SceneManager.LoadScene("LobbyScene");
    }

    public void JoinGame()
    {
        LobbyNetworkStarter.startAsHost = false;
        SceneManager.LoadScene("LobbyScene");
    }


    public void BackToMainMenu()
    {
        hostJoinPanel.SetActive(false); // Hide Host/Join/Quit panel
    }


    public void QuitGame()
    {
        Application.Quit();
    }
}
