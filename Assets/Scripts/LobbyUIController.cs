using UnityEngine;
using System.Linq;

public class LobbyUIController : MonoBehaviour
{
    public PlayerSlotUI[] slots;
    public CharacterDataSO[] characters;
    public GameObject cardPrefab;

    void Start()
    {
        InvokeRepeating(nameof(Refresh), 0.3f, 0.3f);
    }

    void Refresh()
    {
        var players = FindObjectsByType<PlayerNetwork>(FindObjectsSortMode.None)
            .OrderBy(p => p.SlotIndex.Value)
            .ToList();

        for (int i = 0; i < slots.Length; i++)
        {
            if (i >= players.Count)
            {
                slots[i].Clear();
                continue;
            }

            var slot = slots[i];
            var p = players[i];

            // Create card if missing
            if (slot.cardHolder.childCount == 0)
            {
                var cardObj = Instantiate(cardPrefab, slot.cardHolder);
                var cardUI = cardObj.GetComponent<PlayerCardUI>();
                cardUI.ownerId = p.OwnerClientId;
                SetupCardControls(cardUI, p);
            }

            UpdateSlot(slot, p);
        }
    }

    void SetupCardControls(PlayerCardUI ui, PlayerNetwork p)
    {
        bool isLocal = p.OwnerClientId == PlayerNetwork.LocalPlayer.OwnerClientId;
        ui.ShowControls(isLocal);

        if (!isLocal) return;

        ui.prev.onClick.RemoveAllListeners();
        ui.next.onClick.RemoveAllListeners();

        ui.prev.onClick.AddListener(() => Change(-1));
        ui.next.onClick.AddListener(() => Change(1));
    }

    void UpdateSlot(PlayerSlotUI slot, PlayerNetwork p)
    {
        var cardUI = slot.cardHolder.GetComponentInChildren<PlayerCardUI>();
        int idx = Mathf.Clamp(p.SelectedCharacterIndex.Value, 0, characters.Length - 1);
        var data = characters[idx];

        cardUI.SetData(data.portrait, data.characterName, p.IsReady.Value);

        bool isLocal = (p.OwnerClientId == PlayerNetwork.LocalPlayer.OwnerClientId);

        uiToggle(cardUI.prev, isLocal && !p.IsReady.Value);
        uiToggle(cardUI.next, isLocal && !p.IsReady.Value);
    }

    void Change(int dir)
    {
        var me = PlayerNetwork.LocalPlayer;
        if (me == null || me.IsReady.Value) return;

        int current = me.SelectedCharacterIndex.Value;
        int next = current;

        // Search for a free character (not locked by others)
        for (int i = 0; i < characters.Length; i++)
        {
            next = (next + dir + characters.Length) % characters.Length;
            if (!IsCharacterUsedAndLocked(next))
                break;
        }

        me.SetCharacterIndexServerRpc(next);
    }

    bool IsCharacterUsedAndLocked(int index)
    {
        var players = FindObjectsByType<PlayerNetwork>(FindObjectsSortMode.None);

        foreach (var p in players)
        {
            if (p.OwnerClientId == PlayerNetwork.LocalPlayer.OwnerClientId)
                continue;

            if (p.IsReady.Value && p.SelectedCharacterIndex.Value == index)
                return true;
        }
        return false;
    }

    void uiToggle(UnityEngine.UI.Button b, bool enabled)
    {
        if (b != null) b.interactable = enabled;
    }
}
