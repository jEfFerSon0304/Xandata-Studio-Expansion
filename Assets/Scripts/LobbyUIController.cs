using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using UnityEngine.UI;

public class LobbyUIController : NetworkBehaviour
{
    [Header("UI References")]
    public Transform cardContainer;                // Horizontal layout parent
    public GameObject characterSelectionCardPrefab;

    [Header("Data")]
    public CharacterDataSO[] allCharacters;

    private Dictionary<ulong, PlayerCardUI> playerCards = new();

    void Start()
    {
        InvokeRepeating(nameof(RefreshLobby), 0.3f, 0.3f);
    }

    void RefreshLobby()
    {
        var players = FindObjectsOfType<PlayerNetwork>();

        // Ensure one card per connected player
        foreach (var player in players)
        {
            if (!playerCards.ContainsKey(player.OwnerClientId))
            {
                var newCard = Instantiate(characterSelectionCardPrefab, cardContainer);
                var cardUI = newCard.GetComponent<PlayerCardUI>();

                cardUI.ownerClientId = player.OwnerClientId;
                cardUI.isLocal = player.IsOwner;
                cardUI.EnableButtons(player.IsOwner);

                // Connect buttons to local player's actions only
                if (player.IsOwner)
                {
                    cardUI.prevButton.onClick.AddListener(() => OnPrevClicked());
                    cardUI.nextButton.onClick.AddListener(() => OnNextClicked());
                }

                playerCards[player.OwnerClientId] = cardUI;
            }

            UpdateCard(player);
        }

        // Remove cards for disconnected players
        CleanUpDisconnected(players);
    }

    void UpdateCard(PlayerNetwork player)
    {
        if (!playerCards.ContainsKey(player.OwnerClientId)) return;

        var card = playerCards[player.OwnerClientId];
        int charIndex = player.SelectedCharacterIndex.Value;
        bool ready = player.IsReady.Value;

        // Get character data safely
        CharacterDataSO data = null;
        if (charIndex >= 0 && charIndex < allCharacters.Length)
            data = allCharacters[charIndex];

        if (data != null)
            card.SetCharacter(data.portrait, data.characterName);
        else
            card.SetCharacter(null, "—");

        card.SetReadyVisual(ready);
    }

    void OnPrevClicked()
    {
        if (PlayerNetwork.LocalPlayer == null) return;

        var currentIndex = PlayerNetwork.LocalPlayer.SelectedCharacterIndex.Value;
        int newIndex = (currentIndex - 1 + allCharacters.Length) % allCharacters.Length;
        PlayerNetwork.LocalPlayer.SetCharacterIndexServerRpc(newIndex);
    }

    void OnNextClicked()
    {
        if (PlayerNetwork.LocalPlayer == null) return;

        var currentIndex = PlayerNetwork.LocalPlayer.SelectedCharacterIndex.Value;
        int newIndex = (currentIndex + 1) % allCharacters.Length;
        PlayerNetwork.LocalPlayer.SetCharacterIndexServerRpc(newIndex);
    }

    void CleanUpDisconnected(PlayerNetwork[] players)
    {
        var ids = new List<ulong>(playerCards.Keys);
        foreach (var id in ids)
        {
            bool stillConnected = false;
            foreach (var p in players)
            {
                if (p.OwnerClientId == id) { stillConnected = true; break; }
            }
            if (!stillConnected)
            {
                Destroy(playerCards[id].gameObject);
                playerCards.Remove(id);
            }
        }
    }
}
