using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System.Linq;

public class LobbyUIController : NetworkBehaviour
{
    [Header("UI References")]
    public Transform cardContainer;
    public GameObject characterSelectionCardPrefab;

    [Header("Character Data")]
    public CharacterDataSO[] allCharacters;

    private Dictionary<ulong, PlayerCardUI> playerCards = new();

    void Start()
    {
        PlayerNetwork.OnAnyReadyStateChanged += RefreshLobbyInstant;
        InvokeRepeating(nameof(RefreshLobby), 0.3f, 0.3f);
    }

    void OnDestroy()
    {
        PlayerNetwork.OnAnyReadyStateChanged -= RefreshLobbyInstant;
    }

    void RefreshLobbyInstant()
    {
        RefreshLobby();
    }

    void RefreshLobby()
    {
        var players = FindObjectsOfType<PlayerNetwork>().OrderBy(p => p.SlotIndex.Value).ToList();

        // Create cards for new players
        foreach (var player in players)
        {
            if (!playerCards.ContainsKey(player.OwnerClientId))
            {
                var newCard = Instantiate(characterSelectionCardPrefab, cardContainer);
                var cardUI = newCard.GetComponent<PlayerCardUI>();

                cardUI.ownerClientId = player.OwnerClientId;
                cardUI.isLocal = player.IsOwner;

                // Assign player label using SlotIndex (P1–P4)
                string label = $"P{player.SlotIndex.Value + 1}";
                cardUI.SetPlayerLabel(label);

                // Hide navigation buttons for others
                cardUI.ShowNavigationButtons(player.IsOwner);

                // Add button listeners only for local player
                if (player.IsOwner)
                {
                    cardUI.prevButton.onClick.AddListener(() => OnPrevClicked());
                    cardUI.nextButton.onClick.AddListener(() => OnNextClicked());
                }

                playerCards[player.OwnerClientId] = cardUI;
            }

            UpdateCard(player);
        }

        // Remove disconnected players
        CleanDisconnected(players);
    }

    void UpdateCard(PlayerNetwork player)
    {
        if (!playerCards.ContainsKey(player.OwnerClientId)) return;
        var card = playerCards[player.OwnerClientId];

        int charIndex = player.SelectedCharacterIndex.Value;
        bool ready = player.IsReady.Value;

        // Load character data
        CharacterDataSO data = null;
        if (charIndex >= 0 && charIndex < allCharacters.Length)
            data = allCharacters[charIndex];

        // Apply visuals
        if (data != null)
            card.SetCharacter(data.portrait, data.characterName);
        else
            card.SetCharacter(null, "—");

        card.SetReadyVisual(ready);
    }

    void OnPrevClicked()
    {
        if (PlayerNetwork.LocalPlayer == null) return;

        int currentIndex = PlayerNetwork.LocalPlayer.SelectedCharacterIndex.Value;
        int newIndex = (currentIndex - 1 + allCharacters.Length) % allCharacters.Length;
        PlayerNetwork.LocalPlayer.SetCharacterIndexServerRpc(newIndex);
    }

    void OnNextClicked()
    {
        if (PlayerNetwork.LocalPlayer == null) return;

        int currentIndex = PlayerNetwork.LocalPlayer.SelectedCharacterIndex.Value;
        int newIndex = (currentIndex + 1) % allCharacters.Length;
        PlayerNetwork.LocalPlayer.SetCharacterIndexServerRpc(newIndex);
    }

    void CleanDisconnected(List<PlayerNetwork> currentPlayers)
    {
        var ids = playerCards.Keys.ToList();
        foreach (var id in ids)
        {
            if (!currentPlayers.Any(p => p.OwnerClientId == id))
            {
                Destroy(playerCards[id].gameObject);
                playerCards.Remove(id);
            }
        }
    }
}
