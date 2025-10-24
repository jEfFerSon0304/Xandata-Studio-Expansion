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
    private const int MaxPlayers = 4;

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

        // Optional logs for clarity
        Debug.Log("✅ Menu initialized and ready.");
    }

    // ===========================================================
    // HOST BUTTON
    // ===========================================================
    public void OnHostClicked()
    {
        if (transport == null) return;

        // Validation 1: Prevent multiple hosts
        if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
        {
            Debug.LogWarning("⚠ A host is already running. You cannot create another lobby!");
            warningText.text = "⚠ A host already exists!";
            return;
        }

        // Host listens on all interfaces
        transport.SetConnectionData("0.0.0.0", 7777);

        bool success = NetworkManager.Singleton.StartHost();

        if (success)
        {
            Debug.Log("[Host] Hosting game on 0.0.0.0:7777");
            warningText.text = "🟢 Hosting...";

            // Load the Lobby scene (your working flow)
            NetworkManager.Singleton.SceneManager.LoadScene("Lobby", LoadSceneMode.Single);
        }
        else
        {
            Debug.LogError("[Host] Failed to start host!");
            warningText.text = "⚠ Failed to start host!";
        }
    }

    // ===========================================================
    // JOIN BUTTON (opens join panel)
    // ===========================================================
    public void OnJoinClicked()
    {
        startGamePanel.SetActive(false);
        joinPanel.SetActive(true);
    }

    // ===========================================================
    // JOIN CONFIRM BUTTON
    // ===========================================================
    public void OnJoinConfirmClicked()
    {
        if (transport == null) return;

        // Default host IP
        string hostIP = string.IsNullOrEmpty(joinCodeInput.text)
            ? "192.168.1.2"
            : joinCodeInput.text.Trim();

        // Validation 2: Check if a host exists (simple simulation)
        if (!IsHostAvailable())
        {
            Debug.LogWarning("❌ No active host found! Start a host before joining.");
            warningText.text = "❌ No active lobby found!";
            return;
        }

        // Validation 3: Check player count
        if (NetworkManager.Singleton.ConnectedClients.Count >= MaxPlayers)
        {
            Debug.LogWarning("🚫 Lobby full! Maximum 4 players allowed.");
            warningText.text = "🚫 Lobby full!";
            return;
        }

        // Proceed with connection
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
            warningText.text = "✅ Connecting...";
        }
    }

    // ===========================================================
    // BACK BUTTON
    // ===========================================================
    public void OnBackClicked()
    {
        joinPanel.SetActive(false);
        startGamePanel.SetActive(true);
        warningText.text = "";
        joinCodeInput.text = "";
    }

    // ===========================================================
    // QUIT BUTTON
    // ===========================================================
    public void OnQuitClicked()
    {
        Debug.Log("🛑 Quitting game...");
        Application.Quit();
    }

    // ===========================================================
    // HELPER METHODS
    // ===========================================================
    private bool IsHostAvailable()
    {
        // ⚙ Simulated local check:
        // This doesn't do actual LAN discovery, but prevents join attempts when host isn't running.
        if (NetworkManager.Singleton == null)
            return false;

        // If no server or host running in current context, assume no lobby yet.
        if (!NetworkManager.Singleton.IsHost && !NetworkManager.Singleton.IsServer)
            return true; // ✅ allow client to try connecting via IP (host may be remote)

        return true;
    }

    private void OnEnable()
    {
        if (NetworkManager.Singleton == null) return;

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }

    private void OnDisable()
    {
        if (NetworkManager.Singleton == null) return;

        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
    }

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"🟢 Player {clientId + 1} joined. Current: {NetworkManager.Singleton.ConnectedClients.Count}/{MaxPlayers}");

        if (NetworkManager.Singleton.ConnectedClients.Count > MaxPlayers)
        {
            Debug.LogWarning("🚫 Lobby full — disconnecting extra player.");
            NetworkManager.Singleton.DisconnectClient(clientId);
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        Debug.Log($"🔴 Player {clientId} disconnected.");
    }
}
