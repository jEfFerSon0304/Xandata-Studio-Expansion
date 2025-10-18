using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class CharacterSelectionStore : NetworkBehaviour
{
    public static CharacterSelectionStore Instance;

    public bool[] characterLocked; // tracks which characters are locked
    public Dictionary<ulong, PlayerData> players = new Dictionary<ulong, PlayerData>();

    private void Awake()
    {
        Instance = this;
        characterLocked = new bool[4]; // adjust if more characters
    }

    public bool IsCharacterLocked(int index)
    {
        if (index < 0 || index >= characterLocked.Length) return true;
        return characterLocked[index];
    }

    public void AssignPlayerToSlot(ulong clientId)
    {
        // Assign first available slot
        int slotIndex = -1;
        for (int i = 0; i < 4; i++)
        {
            if (!players.ContainsKey((ulong)i))
            {
                slotIndex = i;
                break;
            }
        }

        if (slotIndex == -1) return; // lobby full

        // Assign first available character
        int characterIndex = -1;
        for (int i = 0; i < characterLocked.Length; i++)
        {
            if (!characterLocked[i])
            {
                characterIndex = i;
                characterLocked[i] = true;
                break;
            }
        }

        // Add player data
        players.Add(clientId, new PlayerData
        {
            clientId = clientId,
            slotIndex = slotIndex,
            characterIndex = characterIndex,
            isReady = false
        });

        // Update all clients
        UpdatePlayerClientRpc(clientId, slotIndex, characterIndex);
    }

    [ClientRpc]
    void UpdatePlayerClientRpc(ulong clientId, int slotIndex, int characterIndex)
    {
        LobbyUIManager.Instance.UpdateSlotUI(slotIndex, clientId, characterIndex);
    }

    public void SetReady(ulong clientId, bool ready)
    {
        if (players.ContainsKey(clientId))
        {
            players[clientId].isReady = ready;
        }
    }
}

public class PlayerData
{
    public ulong clientId;
    public int slotIndex;
    public int characterIndex;
    public bool isReady;
}
