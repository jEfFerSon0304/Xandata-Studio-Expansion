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

    [Header("Join Settings")]
    public TMP_InputField joinCodeInput;
    public TMP_Text warningText;

    private UnityTransport transport;

    private void Start()
    {
        // Ensure correct state at start
        startGamePanel.SetActive(true);
        joinPanel.SetActive(false);

        transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        if (transport == null)
        {
            Debug.LogError("⚠ UnityTransport not found on NetworkManager!");
        }
    }

    // -----------------------------
    // HOST BUTTON CLICKED
    // -----------------------------
    public void OnHostClicked()
    {
        if (transport == null) return;

        // Host listens on all network interfaces
        transport.SetConnectionData("0.0.0.0", 7777);

        bool success = NetworkManager.Singleton.StartHost();

        if (success)
        {
            Debug.Log("[Host] Hosting game on 0.0.0.0:7777");
            // Use Netcode scene load to sync all players
            NetworkManager.Singleton.SceneManager.LoadScene("Lobby", LoadSceneMode.Single);
        }
        else
        {
            Debug.LogError("[Host] Failed to start host!");
            warningText.text = "⚠ Failed to start host!";
        }
    }

    // -----------------------------
    // JOIN BUTTON CLICKED
    // -----------------------------
    public void OnJoinClicked()
    {
        startGamePanel.SetActive(false);
        joinPanel.SetActive(true);
    }

    // -----------------------------
    // JOIN CONFIRM BUTTON CLICKED
    // -----------------------------
    public void OnJoinConfirmClicked()
    {
        if (transport == null) return;

        // Phone connects to your PC (host IP)
        string hostIP = string.IsNullOrEmpty(joinCodeInput.text)
            ? "192.168.1.2" // Default PC host IP
            : joinCodeInput.text; // Optional manual input override

        transport.SetConnectionData(hostIP, 7777);

        bool success = NetworkManager.Singleton.StartClient();

        if (!success)
        {
            Debug.LogError("[Client] Failed to connect to host!");
            warningText.text = "⚠ Connection failed!";
        }
        else
        {
            Debug.Log($"[Client] Connecting to host at {hostIP}:7777");
        }
    }

    // -----------------------------
    // BACK BUTTON CLICKED
    // -----------------------------
    public void OnBackClicked()
    {
        joinPanel.SetActive(false);
        startGamePanel.SetActive(true);
        warningText.text = "";
        joinCodeInput.text = "";
    }

    // -----------------------------
    // QUIT BUTTON CLICKED
    // -----------------------------
    public void OnQuitClicked()
    {
        Application.Quit();
    }
}
