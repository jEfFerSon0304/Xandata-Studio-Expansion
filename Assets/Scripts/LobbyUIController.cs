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
        if (GameDatabase.Instance == null) return;

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

            if (slot.cardHolder.childCount == 0)
            {
                var card = Instantiate(cardPrefab);
                var ui = card.GetComponent<PlayerCardUI>();
                ui.ownerId = p.OwnerClientId;
                ui.ShowControls(p.IsOwner);
                slot.SetCard(card);

                if (p.IsOwner)
                {
                    ui.prev.onClick.RemoveAllListeners();
                    ui.next.onClick.RemoveAllListeners();

                    ui.prev.onClick.AddListener(() => Change(-1));
                    ui.next.onClick.AddListener(() => Change(1));
                }
            }

            UpdateSlot(slot, p);
        }
    }

    void UpdateSlot(PlayerSlotUI slot, PlayerNetwork p)
    {
        var idx = p.SelectedCharacterIndex.Value;
        if (idx < 0 || idx >= characters.Length) return;

        var cardUI = slot.cardHolder.GetComponentInChildren<PlayerCardUI>();
        var data = characters[idx];

        cardUI.SetData(data.portrait, data.characterName,
            p.State != null && p.State.IsReady.Value);
    }

    void Change(int dir)
    {
        if (PlayerNetwork.LocalPlayer == null) return;
        var me = PlayerNetwork.LocalPlayer;
        if (me.State != null && me.State.IsReady.Value) return;

        int newIndex = (me.SelectedCharacterIndex.Value + dir + characters.Length) % characters.Length;
        me.SetCharacterIndexServerRpc(newIndex);
    }
}
