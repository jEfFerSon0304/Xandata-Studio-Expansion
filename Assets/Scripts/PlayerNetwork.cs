using UnityEngine;
using Unity.Netcode;
using System;
using System.Linq;

public class PlayerNetwork : NetworkBehaviour
{
    public static PlayerNetwork LocalPlayer;
    public static event Action OnAnyStateChanged;

    public NetworkVariable<int> SelectedCharacterIndex = new(
        -1,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public NetworkVariable<bool> IsReady = new(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    void Awake() => DontDestroyOnLoad(gameObject);

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
            LocalPlayer = this;

        // Only server can write to NetworkVariables
        if (IsServer)
        {
            IsReady.Value = false;
            Debug.Log($"[Server] Player {OwnerClientId} starts unready ⏳");
        }

        // Subscribe to ready-state changes for UI updates
        IsReady.OnValueChanged += (_, __) => OnAnyStateChanged?.Invoke();
    }


    [ServerRpc(RequireOwnership = false)]
    public void SetCharacterIndexServerRpc(int index, ServerRpcParams rpc = default)
    {
        if (!IsServer) return;

        ulong senderId = rpc.Receive.SenderClientId;
        var player = FindObjectsByType<PlayerNetwork>(FindObjectsSortMode.None)
            .FirstOrDefault(p => p.OwnerClientId == senderId);

        if (player == null || player.IsReady.Value) return;

        player.SelectedCharacterIndex.Value = index;
        OnAnyStateChanged?.Invoke();
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetReadyServerRpc(bool ready, ServerRpcParams rpc = default)
    {
        if (!IsServer) return;

        ulong senderId = rpc.Receive.SenderClientId;
        var player = FindObjectsByType<PlayerNetwork>(FindObjectsSortMode.None)
            .FirstOrDefault(p => p.OwnerClientId == senderId);

        if (player == null) return;

        if (ready && player.SelectedCharacterIndex.Value < 0)
        {
            Debug.Log($"[Server] Player {senderId} tried to ready without selecting a character ❌");
            return;
        }

        player.IsReady.Value = ready;
        Debug.Log($"[Server] Player {senderId} Ready = {ready}");
        OnAnyStateChanged?.Invoke();
    }

    public static void NotifyUpdate() => OnAnyStateChanged?.Invoke();
}
