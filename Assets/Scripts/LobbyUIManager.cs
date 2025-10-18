using UnityEngine;
using UnityEngine.UI;

public class LobbyUIManager : MonoBehaviour
{
    public static LobbyUIManager Instance;

    public GameObject[] playerSlots; // Slot1–Slot4 panels
    public Sprite[] characterSprites;
    public string[] characterNames;

    private void Awake()
    {
        Instance = this;
    }

    public void UpdateSlotUI(int slotIndex, ulong clientId, int characterIndex)
    {
        if (slotIndex < 0 || slotIndex >= playerSlots.Length) return;

        var slot = playerSlots[slotIndex];
        var statusText = slot.transform.Find("StatusText").GetComponent<Text>();
        var image = slot.transform.Find("CharacterImagePlaceholder").GetComponent<Image>();

        statusText.text = $"Player {clientId}";
        image.sprite = characterSprites[characterIndex];
    }
}
