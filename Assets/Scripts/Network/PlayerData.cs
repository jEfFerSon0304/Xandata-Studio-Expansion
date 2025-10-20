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

        // Create the UI card for this player
        if (LobbyManager.Instance != null)
        {
            LobbyManager.Instance.SpawnPlayerCard(OwnerClientId);
        }
        else
        {
            Debug.LogWarning("[PlayerData] LobbyManager.Instance is null!");
        }

        // Listen for character index changes to update UI automatically
        CharacterIndex.OnValueChanged += OnCharacterIndexChanged;

        IsReady.OnValueChanged += OnReadyStateChanged;

        // Update initial display
        if (LobbyManager.Instance != null)
            LobbyManager.Instance.UpdatePlayerCharacterLocal(OwnerClientId, CharacterIndex.Value);
    }

    private void OnReadyStateChanged(bool previous, bool current)
    {
        if (LobbyManager.Instance != null)
        {
            LobbyManager.Instance.UpdatePlayerReadyStatus(OwnerClientId, current);
        }
    }


    public override void OnNetworkDespawn()
    {
        CharacterIndex.OnValueChanged -= OnCharacterIndexChanged;
    }

    private void OnCharacterIndexChanged(int previous, int current)
    {
        if (LobbyManager.Instance != null)
        {
            LobbyManager.Instance.UpdatePlayerCharacterLocal(OwnerClientId, current);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetReadyStateServerRpc(bool ready)
    {
        IsReady.Value = ready;
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestCharacterChangeServerRpc(bool next, ServerRpcParams rpcParams = default)
    {
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

        int newIndex = next
            ? (CharacterIndex.Value + 1) % count
            : (CharacterIndex.Value - 1 + count) % count;

        CharacterIndex.Value = newIndex;

        Debug.Log($"[Server] Player {OwnerClientId} switched to character index {newIndex}");
    }
}
