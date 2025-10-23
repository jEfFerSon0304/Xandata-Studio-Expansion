using UnityEngine;
using Unity.Netcode;
using System;
using System.Linq;
using System.Collections.Generic;

public class PlayerNetwork : NetworkBehaviour
{
    public static PlayerNetwork LocalPlayer;
    public static event Action OnAnyReadyStateChanged;

    public static List<PlayerNetwork> AllPlayers = new List<PlayerNetwork>();

    public NetworkVariable<int> SlotIndex = new(
        -1,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    public NetworkVariable<int> SelectedCharacterIndex = new(
        -1,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    public NetworkVariable<bool> IsReady = new(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
            LocalPlayer = this;

        if (IsServer)
        {
            // Add to list
            if (!AllPlayers.Contains(this))
                AllPlayers.Add(this);

            // Assign slots dynamically
            ReassignAllSlots();

            // Listen for disconnects
            NetworkManager.OnClientDisconnectCallback -= HandleClientDisconnect;
            NetworkManager.OnClientDisconnectCallback += HandleClientDisconnect;
        }

        // Subscribe to ready-state changes
        IsReady.OnValueChanged += (_, __) => OnAnyReadyStateChanged?.Invoke();
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            if (AllPlayers.Contains(this))
                AllPlayers.Remove(this);

            ReassignAllSlots();
        }
    }

    private void HandleClientDisconnect(ulong clientId)
    {
        if (!IsServer) return;

        // Remove any disconnected player objects
        AllPlayers.RemoveAll(p => p == null || !p.IsSpawned);

        ReassignAllSlots();
    }

    // 🔹 Reassigns P1, P2, P3, P4 dynamically based on current order
    private void ReassignAllSlots()
    {
        var activePlayers = AllPlayers
            .Where(p => p != null && p.IsSpawned)
            .OrderBy(p => p.OwnerClientId)
            .ToList();

        for (int i = 0; i < activePlayers.Count; i++)
        {
            activePlayers[i].SlotIndex.Value = i;
            Debug.Log($"[Server] Reassigned Slot P{i + 1} to Client {activePlayers[i].OwnerClientId}");
        }

        OnAnyReadyStateChanged?.Invoke(); // refresh UI
    }

    // 🔹 Character Selection
    [ServerRpc(RequireOwnership = false)]
    public void SetCharacterIndexServerRpc(int index, ServerRpcParams rpcParams = default)
    {
        if (!IsServer) return;

        ulong sender = rpcParams.Receive.SenderClientId;
        var player = AllPlayers.FirstOrDefault(p => p.OwnerClientId == sender);
        if (player == null) return;

        if (player.IsReady.Value)
        {
            Debug.Log($"[Server] Player {sender} cannot change character when locked.");
            return;
        }

        bool locked = AllPlayers.Any(p => p.IsReady.Value && p.SelectedCharacterIndex.Value == index);
        if (locked)
        {
            Debug.Log($"[Server] Character index {index} already taken by another ready player.");
            return;
        }

        player.SelectedCharacterIndex.Value = index;
    }

    // 🔹 Ready/Unready
    [ServerRpc(RequireOwnership = false)]
    public void SetReadyServerRpc(bool ready, ServerRpcParams rpcParams = default)
    {
        if (!IsServer) return;

        ulong sender = rpcParams.Receive.SenderClientId;
        var player = AllPlayers.FirstOrDefault(p => p.OwnerClientId == sender);
        if (player == null) return;

        if (ready)
        {
            if (player.SelectedCharacterIndex.Value < 0)
            {
                Debug.Log($"[Server] Player {sender} must select a character first!");
                return;
            }

            bool taken = AllPlayers.Any(p => p.IsReady.Value && p.SelectedCharacterIndex.Value == player.SelectedCharacterIndex.Value);
            if (taken)
            {
                Debug.Log($"[Server] Player {sender} tried to ready a taken character.");
                return;
            }
        }

        player.IsReady.Value = ready;
        Debug.Log($"[Server] Player {sender} Ready = {ready}");
        OnAnyReadyStateChanged?.Invoke();
    }
}
