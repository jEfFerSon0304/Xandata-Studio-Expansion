using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour

{
    public static LobbyManager Instance;

    [Header("UI References")]
    public Transform playerCardsPanel;
    public GameObject playerCardPrefab;
    public GameObject startGameButton;
    public GameObject readyButton;   // 👈 add this

    private Dictionary<ulong, PlayerCardUI> playerCards = new();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // Add callbacks for host
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }

        // Delay the button visibility check slightly, to give Netcode time to initialize
        Invoke(nameof(RefreshButtonVisibility), 0.1f);
    }

    private void RefreshButtonVisibility()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogWarning("⚠️ NetworkManager not found — cannot set button visibility.");
            return;
        }

        bool isHost = NetworkManager.Singleton.IsHost;

        if (isHost)
        {
            Debug.Log("👑 Host confirmed — showing StartGameButton");
            if (startGameButton != null) startGameButton.SetActive(true);
            if (readyButton != null) readyButton.SetActive(false);
        }
        else
        {
            Debug.Log("🙋 Client confirmed — showing ReadyButton");
            if (readyButton != null) readyButton.SetActive(true);
            if (startGameButton != null) startGameButton.SetActive(false);
        }
    }



    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"🧩 OnClientConnected triggered for ClientId={clientId}, host={NetworkManager.Singleton.IsHost}");

        // Server creates PlayerData
        if (NetworkManager.Singleton.IsServer)
        {
            var playerDataPrefab = Resources.Load<GameObject>("Prefabs/Network/PlayerData");
            if (playerDataPrefab)
            {
                var obj = Instantiate(playerDataPrefab);
                obj.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
            }
        }

        // Create the UI card locally on all clients
        SpawnPlayerCard(clientId);
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (playerCards.ContainsKey(clientId))
        {
            Destroy(playerCards[clientId].gameObject);
            playerCards.Remove(clientId);
        }
    }

    // LOCAL ONLY — spawns card UI, not networked
    public void SpawnPlayerCard(ulong clientId)
    {
        if (playerCardsPanel == null || playerCardPrefab == null)
        {
            Debug.LogError("❌ Missing PlayerCardsPanel or PlayerCardPrefab reference!");
            return;
        }

        if (playerCards.ContainsKey(clientId))
        {
            Debug.Log($"ℹ️ Player card already exists for client {clientId}");
            return;
        }

        Debug.Log($"🎴 [LobbyManager] Spawning PlayerCard for client {clientId}");
        GameObject cardObj = Instantiate(playerCardPrefab, playerCardsPanel);
        var cardUI = cardObj.GetComponent<PlayerCardUI>();
        cardUI.AssignClient(clientId);
        playerCards[clientId] = cardUI;
    }

    // LOCAL UPDATE called when PlayerData.CharacterIndex changes
    public void UpdatePlayerCharacterLocal(ulong clientId, int charIndex)
    {
        if (!playerCards.ContainsKey(clientId))
        {
            Debug.LogWarning($"⚠️ No PlayerCard found for client {clientId} yet — skipping update.");
            return;
        }

        var card = playerCards[clientId];
        var data = CharacterSelectionStore.Instance.GetCharacterByIndex(charIndex);

        if (data == null)
        {
            Debug.LogWarning($"⚠️ Character index {charIndex} invalid or missing.");
            return;
        }

        // Update UI locally
        card.UpdateCharacterDisplay(charIndex);

        Debug.Log($"🎴 [LobbyManager] Updated Player {clientId}'s character to {data.characterName}");
    }

    public void TryStartGame()
    {
        if (!NetworkManager.Singleton.IsHost) return;

        foreach (var kvp in playerCards)
        {
            if (!kvp.Value.IsReady)
            {
                Debug.Log("Not all players are ready!");
                return;
            }
        }

        Debug.Log("✅ All players ready! Starting game...");
        //NetworkManager.SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
    }

    public void UpdatePlayerReadyStatus(ulong clientId, bool isReady)
    {
        if (!playerCards.ContainsKey(clientId))
        {
            Debug.LogWarning($"⚠️ No PlayerCard found for client {clientId} when updating ready status.");
            return;
        }

        var card = playerCards[clientId];
        card.UpdateReadyStatus(isReady);

        Debug.Log($"✅ Updated Player {clientId} ready status → {(isReady ? "Ready" : "Selecting...")}");
    }

}
