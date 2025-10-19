using Unity.Netcode;
using UnityEngine;

public class PlayerData : NetworkBehaviour
{
    public static PlayerData LocalPlayer;

    public NetworkVariable<int> CharacterIndex = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<bool> IsReady = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        Debug.Log($"[PlayerData] Spawned for ClientId: {OwnerClientId}");

        if (IsOwner)
            LocalPlayer = this;

        // Try to notify LobbyManager
        if (LobbyManager.Instance != null)
        {
            LobbyManager.Instance.SpawnPlayerCard(OwnerClientId);
        }
        else
        {
            Debug.LogWarning("[PlayerData] LobbyManager.Instance is null!");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetCharacterIndexServerRpc(int index)
    {
        CharacterIndex.Value = index;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetReadyStateServerRpc(bool ready)
    {
        IsReady.Value = ready;
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestCharacterChangeServerRpc(bool next, ServerRpcParams rpcParams = default)
    {
        // Make sure our character store exists
        if (CharacterSelectionStore.Instance == null)
        {
            Debug.LogError("[PlayerData] CharacterSelectionStore.Instance is missing!");
            return;
        }

        int count = CharacterSelectionStore.Instance.characters.Count;
        if (count == 0)
        {
            Debug.LogWarning("[PlayerData] No characters available in store!");
            return;
        }

        // Cycle through the list
        int newIndex = next
            ? (CharacterIndex.Value + 1) % count
            : (CharacterIndex.Value - 1 + count) % count;

        CharacterIndex.Value = newIndex;

        Debug.Log($"[Server] Player {OwnerClientId} switched to character index {newIndex}");

        // Update everyone’s UI (host + clients)
        if (LobbyManager.Instance != null)
            LobbyManager.Instance.UpdatePlayerCharacterClientRpc(OwnerClientId, newIndex);
        else
            Debug.LogWarning("[PlayerData] LobbyManager.Instance is null when updating character!");
    }

}
