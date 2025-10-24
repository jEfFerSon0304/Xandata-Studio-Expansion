using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System.Linq;

public class LobbyUIController : NetworkBehaviour
{
    [Header("UI References")]
    public Transform container;                // Parent that holds player cards
    public GameObject cardPrefab;              // Prefab for each player's card
    public CharacterDataSO[] allCharacters;    // Array of available characters

    private Dictionary<ulong, PlayerCardUI> cards = new();

    void Start()
    {
        PlayerNetwork.OnAnyStateChanged += RefreshInstant;
        InvokeRepeating(nameof(Refresh), 0.3f, 0.3f);
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        PlayerNetwork.OnAnyStateChanged -= RefreshInstant;
    }

    void RefreshInstant() => Refresh();

    void Refresh()
    {
        var players = FindObjectsByType<PlayerNetwork>(FindObjectsSortMode.None).OrderBy(p => p.OwnerClientId).ToList();

        // Create missing player cards
        foreach (var p in players)
        {
            if (!cards.ContainsKey(p.OwnerClientId))
            {
                var card = Instantiate(cardPrefab, container).GetComponent<PlayerCardUI>();
                card.ownerId = p.OwnerClientId;

                // Only show control buttons for local player
                card.ShowControls(p.IsOwner);
                cards[p.OwnerClientId] = card;

                // ✅ Add event listeners for local player only
                if (p.IsOwner)
                {
                    if (card.prev != null)
                        card.prev.onClick.AddListener(() => Change(-1));

                    if (card.next != null)
                        card.next.onClick.AddListener(() => Change(1));
                }

                // Assign initial character (server only)
                if (IsServer && p.SelectedCharacterIndex.Value < 0)
                    AssignFirstAvailableCharacter(p);

            }

            UpdateCard(p);
        }

        // Remove cards for disconnected players
        var ids = cards.Keys.ToList();
        foreach (var id in ids)
        {
            if (!players.Any(p => p.OwnerClientId == id))
            {
                Destroy(cards[id].gameObject);
                cards.Remove(id);
            }
        }
    }

    void UpdateCard(PlayerNetwork p)
    {
        if (!cards.ContainsKey(p.OwnerClientId)) return;

        var card = cards[p.OwnerClientId];
        var data = (p.SelectedCharacterIndex.Value >= 0 && p.SelectedCharacterIndex.Value < allCharacters.Length)
            ? allCharacters[p.SelectedCharacterIndex.Value]
            : null;

        bool isLocked = p.IsReady.Value;

        card.SetData(data?.portrait, data?.characterName ?? "—", isLocked);

        // Disable local controls when locked
        if (p.IsOwner)
        {
            card.prev.interactable = !isLocked;
            card.next.interactable = !isLocked;
        }
    }

    void Change(int dir)
    {
        if (PlayerNetwork.LocalPlayer == null) return;
        if (PlayerNetwork.LocalPlayer.IsReady.Value) return; // can't change while locked

        int cur = PlayerNetwork.LocalPlayer.SelectedCharacterIndex.Value;
        int next = (cur + dir + allCharacters.Length) % allCharacters.Length;

        // Skip locked characters
        var locked = GetLockedIndices();
        while (locked.Contains(next))
            next = (next + dir + allCharacters.Length) % allCharacters.Length;

        PlayerNetwork.LocalPlayer.SetCharacterIndexServerRpc(next);
    }

    void AssignFirstAvailableCharacter(PlayerNetwork player)
    {
        var locked = GetLockedIndices();

        for (int i = 0; i < allCharacters.Length; i++)
        {
            if (!locked.Contains(i))
            {
                player.SetCharacterIndexServerRpc(i);
                break;
            }
        }
    }

    HashSet<int> GetLockedIndices()
    {
        var players = FindObjectsByType<PlayerNetwork>(FindObjectsSortMode.None);
        return new HashSet<int>(
            players.Where(p => p.IsReady.Value)
                   .Select(p => p.SelectedCharacterIndex.Value)
                   .Where(i => i >= 0)
        );
    }
}
