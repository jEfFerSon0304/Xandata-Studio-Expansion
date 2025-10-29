using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    [Header("Panels")]
    public GameObject startGamePanel;
    public GameObject joinPanel;

    [Header("Start Panel UI Elements to Hide/Show")]
    public GameObject[] startUIElements; // assign buttons, header image, etc.

    [Header("Back Button")]
    public GameObject backButton;

    [Header("Join Settings")]
    public TMP_InputField joinCodeInput;
    public TMP_Text warningText;

    private UnityTransport transport;
    private const int MaxPlayers = 4;

    private void Start()
    {
        startGamePanel.SetActive(true);
        joinPanel.SetActive(false);

        // Ensure back button is visible in start panel
        if (backButton != null)
            backButton.SetActive(true);

        // Show all start panel UI elements at start
        foreach (var element in startUIElements)
            element.SetActive(true);

        transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
    }

    public void OnHostClicked()
    {
        if (backButton != null)
            backButton.SetActive(false);

        if (transport == null) return;

        if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
        {
            warningText.text = "⚠ A host already exists!";
            return;
        }

        transport.SetConnectionData("0.0.0.0", 7777);
        if (NetworkManager.Singleton.StartHost())
            NetworkManager.Singleton.SceneManager.LoadScene("Lobby", LoadSceneMode.Single);
    }

    public void OnJoinClicked()
    {
        joinPanel.SetActive(true);

        // Hide start panel UI elements
        foreach (var element in startUIElements)
            element.SetActive(false);

        if (backButton != null)
            backButton.SetActive(true);
    }

    public void OnJoinConfirmClicked()
    {
        if (transport == null) return;

        string hostIP = string.IsNullOrEmpty(joinCodeInput.text) ? "192.168.1.2" : joinCodeInput.text.Trim();

        if (!IsHostAvailable()) { warningText.text = "❌ No active lobby found!"; return; }
        if (NetworkManager.Singleton.ConnectedClients.Count >= MaxPlayers) { warningText.text = "🚫 Lobby full!"; return; }

        transport.SetConnectionData(hostIP, 7777);
        if (!NetworkManager.Singleton.StartClient())
            warningText.text = "⚠ Connection failed!";
        else
            warningText.text = "✅ Connecting...";
    }

    public void OnBackClicked()
    {
        if (joinPanel.activeSelf)
        {
            // Go back to start panel
            joinPanel.SetActive(false);
            joinCodeInput.text = "";
            warningText.text = "";

            // Show start panel UI elements again
            foreach (var element in startUIElements)
                element.SetActive(true);
        }
        else
        {
            // At start panel → load MainMenu scene
            SceneManager.LoadScene("MainMenu");
        }
    }

    public void OnQuitClicked() => Application.Quit();

    private bool IsHostAvailable()
    {
        if (NetworkManager.Singleton == null) return false;
        return !NetworkManager.Singleton.IsHost && !NetworkManager.Singleton.IsServer ? true : true;
    }
}
