using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    [Header("Panels")]
    public GameObject startGamePanel;
    public GameObject joinPanel;

    [Header("Join Settings")]
    public TMP_InputField joinCodeInput;
    public TMP_Text warningText;

    private void Start()
    {
        // Ensure correct state at start
        startGamePanel.SetActive(true);
        joinPanel.SetActive(false);
    }

    public void OnHostClicked()
    {
        NetworkManager.Singleton.StartHost();
        SceneManager.LoadScene("Lobby");
    }

    public void OnJoinClicked()
    {
        // Hide start panel and show join panel
        startGamePanel.SetActive(false);
        joinPanel.SetActive(true);
    }

    public void OnJoinConfirmClicked()
    {
        if (string.IsNullOrEmpty(joinCodeInput.text))
        {
            warningText.text = "⚠ Please enter a join code!";
            return;
        }

        // Example: could use IP or code system here
        bool success = NetworkManager.Singleton.StartClient();

        if (!success)
        {
            warningText.text = "⚠ Invalid Lobby Code!";
        }
        else
        {
            SceneManager.LoadScene("Lobby");
        }
    }

    public void OnBackClicked()
    {
        // Go back to main panel
        joinPanel.SetActive(false);
        startGamePanel.SetActive(true);
        warningText.text = "";
        joinCodeInput.text = "";
    }

    public void OnQuitClicked()
    {
        Application.Quit();
    }
}
