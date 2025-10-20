using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyManager : NetworkBehaviour
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

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }

        // Host sees StartGame; clients see Ready
        if (IsHost)
        {
            if (readyButton != null) readyButton.SetActive(false);
            if (startGameButton != null) startGameButton.SetActive(true);
        }
        else
        {
            if (readyButton != null) readyButton.SetActive(true);
            if (startGameButton != null) startGameButton.SetActive(false);
        }
    }


    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"🧩 OnClientConnected triggered for ClientId={clientId}, host={IsHost}");

        // Server creates PlayerData
        if (IsServer)
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

    public void UpdatePlayerReadyStatus(ulong clientId, bool isReady)
    {
        if (!playerCards.ContainsKey(clientId))
        {
            Debug.LogWarning($"⚠️ No card found for {clientId} when updating ready status.");
            return;
        }

        var card = playerCards[clientId];
        card.UpdateReadyStatus(isReady);
    }

    public void TryStartGame()
    {
        if (!IsHost) return;

        foreach (var kvp in NetworkManager.Singleton.ConnectedClientsList)
        {
            var playerObj = kvp.PlayerObject;
            if (playerObj == null) continue;

            var data = playerObj.GetComponent<PlayerData>();
            if (data == null || !data.IsReady.Value)
            {
                Debug.Log("⏳ Not all players are ready yet!");
                return;
            }
        }

        Debug.Log("✅ All players ready! Starting game...");
        NetworkManager.SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
    }

}
