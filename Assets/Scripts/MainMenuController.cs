using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField joinCodeInput;
    public TMP_Text statusText;
    public GameObject joinPanel;
    public Button hostButton;
    public Button joinButton;
    public Button quitButton;

    void Start()
    {
        joinPanel.SetActive(false);
        hostButton.onClick.AddListener(StartHost);
        joinButton.onClick.AddListener(OpenJoinPanel);
        quitButton.onClick.AddListener(() => Application.Quit());
    }

    void OpenJoinPanel()
    {
        joinPanel.SetActive(true);
    }

    public void StartHost()
    {
        if (NetworkManager.Singleton.StartHost())
        {
            statusText.text = "Hosting...";
            NetworkManager.Singleton.SceneManager.LoadScene("Lobby", LoadSceneMode.Single);
        }
        else
        {
            statusText.text = "Failed to start host.";
        }
    }

    public void JoinLobby()
    {
        if (NetworkManager.Singleton.StartClient())
        {
            statusText.text = "Joining lobby...";
        }
        else
        {
            statusText.text = "Failed to join.";
        }
    }
}
