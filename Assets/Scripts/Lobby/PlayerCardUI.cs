using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Netcode;

public class PlayerCardUI : NetworkBehaviour
{
    [Header("UI References")]
    public TMP_Text playerNameText;
    public TMP_Text characterNameText;
    public Image characterImage;
    public TMP_Text statusText;
    public Button nextButton;
    public Button prevButton;

    private int currentCharacterIndex = 0;
    private bool isReady = false;
    private ulong ownerClientId;

    public bool IsReady => isReady;

    private ulong assignedClientId;
    public ulong AssignedClientId => assignedClientId;


    // Called by LobbyManager when spawning a player card
    public void AssignClient(ulong clientId)
    {
        ownerClientId = clientId;
        playerNameText.text = $"Player {clientId}";

        // Only local player can interact with their card
        bool isLocal = (clientId == NetworkManager.Singleton.LocalClientId);
        nextButton.gameObject.SetActive(isLocal);
        prevButton.gameObject.SetActive(isLocal);
    }

    private void Start()
    {
        // Attach button listeners
        nextButton.onClick.AddListener(() => RequestCharacterChange(true));
        prevButton.onClick.AddListener(() => RequestCharacterChange(false));

        UpdateCharacterDisplay();
    }

    // Local request to change character
    private void RequestCharacterChange(bool next)
    {
        // Prevent remote players from changing others' cards
        if (ownerClientId != NetworkManager.Singleton.LocalClientId)
            return;

        if (PlayerData.LocalPlayer != null)
        {
            // Ask the server to handle the change
            PlayerData.LocalPlayer.RequestCharacterChangeServerRpc(next);
        }
        else
        {
            Debug.LogWarning("[PlayerCardUI] LocalPlayer reference not set!");
        }
    }

    // Update visuals on this card (called from LobbyManager via ClientRpc)
    public void UpdateCharacterDisplay(int index = -1)
    {
        if (index >= 0)
            currentCharacterIndex = index;

        var data = CharacterSelectionStore.Instance.GetCharacterByIndex(currentCharacterIndex);
        if (data == null)
        {
            characterNameText.text = "N/A";
            characterImage.sprite = null;
            return;
        }

        characterNameText.text = data.characterName;
        characterImage.sprite = data.characterIcon;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetReadyServerRpc(bool ready)
    {
        isReady = ready;
        UpdateReadyClientRpc(ready);
    }

    [ClientRpc]
    private void UpdateReadyClientRpc(bool ready)
    {
        isReady = ready;
        statusText.text = ready ? "✅ Ready" : "Selecting...";
    }

    public int GetSelectedCharacterIndex() => currentCharacterIndex;

    public void UpdateReadyStatus(bool ready)
    {
        isReady = ready;
        statusText.text = ready ? "✅ Ready" : "Selecting...";
    }

}
