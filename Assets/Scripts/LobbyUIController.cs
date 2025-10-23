using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System.Linq;

public class LobbyUIController : NetworkBehaviour
{
    public Transform container;
    public GameObject cardPrefab;
    public CharacterDataSO[] allCharacters;

    Dictionary<ulong, PlayerCardUI> cards = new();

    public override void OnDestroy()
    {
        base.OnDestroy();
        PlayerNetwork.OnAnyStateChanged -= RefreshInstant;
    }

    void Start()
    {
        PlayerNetwork.OnAnyStateChanged += RefreshInstant;
        InvokeRepeating(nameof(Refresh), 0.3f, 0.3f);
    }

    void RefreshInstant() => Refresh();

    void Refresh()
    {
        var players = FindObjectsByType<PlayerNetwork>(FindObjectsSortMode.None).ToList();

        foreach (var p in players)
        {
            if (!cards.ContainsKey(p.OwnerClientId))
            {
                var card = Instantiate(cardPrefab, container).GetComponent<PlayerCardUI>();
                card.ownerId = p.OwnerClientId;
                card.ShowControls(p.IsOwner);
                if (p.IsOwner)
                {
                    card.prev.onClick.AddListener(() => Change(-1));
                    card.next.onClick.AddListener(() => Change(1));
                }
                cards[p.OwnerClientId] = card;
            }
            UpdateCard(p);
        }

        // Remove disconnected
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

        card.SetData(data?.portrait, data?.characterName ?? "—", p.IsReady.Value);
    }

    void Change(int dir)
    {
        if (PlayerNetwork.LocalPlayer == null) return;
        int cur = PlayerNetwork.LocalPlayer.SelectedCharacterIndex.Value;
        int next = (cur + dir + allCharacters.Length) % allCharacters.Length;
        PlayerNetwork.LocalPlayer.SetCharacterIndexServerRpc(next);
    }
}
