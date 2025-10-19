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

    private Dictionary<ulong, PlayerCardUI> playerCards = new();

    private void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }

        if (!IsHost)
            startGameButton.SetActive(false);
    }

    private void OnClientConnected(ulong clientId)
    {

        Debug.Log($"OnClientConnected triggered on {NetworkManager.Singleton.LocalClientId}, host={IsHost}");

        SpawnPlayerCardClientRpc(clientId);

        if (IsServer)
        {
            var playerDataPrefab = Resources.Load<GameObject>("Prefabs/Network/PlayerData");
            if (playerDataPrefab)
            {
                var obj = Instantiate(playerDataPrefab);
                obj.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
            }
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (playerCards.ContainsKey(clientId))
        {
            Destroy(playerCards[clientId].gameObject);
            playerCards.Remove(clientId);
        }
    }

    [ClientRpc]
    private void SpawnPlayerCardClientRpc(ulong clientId)
    {
        GameObject cardObj = Instantiate(playerCardPrefab, playerCardsPanel);
        var cardUI = cardObj.GetComponent<PlayerCardUI>();
        cardUI.AssignClient(clientId);

        playerCards[clientId] = cardUI;
    }

    public void TryStartGame()
    {
        if (!IsHost) return;

        foreach (var kvp in playerCards)
        {
            var data = kvp.Value.GetComponent<PlayerCardUI>();
            if (!data.IsReady)
            {
                Debug.Log("Not all players are ready!");
                return;
            }
        }

        Debug.Log("? All players ready! Starting game...");
        NetworkManager.SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
    }

    // Called from PlayerData when a player changes character
    [ClientRpc]
    public void UpdatePlayerCharacterClientRpc(ulong clientId, int charIndex)
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

        // Update UI on every client
        card.characterNameText.text = data.characterName;
        card.characterImage.sprite = data.characterIcon;

        Debug.Log($"🎴 [LobbyManager] Updated Player {clientId}'s character to {data.characterName}");
    }

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

}
