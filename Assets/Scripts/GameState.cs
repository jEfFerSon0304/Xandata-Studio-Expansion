using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class GameState : NetworkBehaviour
{
    public static GameState Instance;

    public NetworkList<ulong> turnOrder = new NetworkList<ulong>();
    public NetworkVariable<int> currentTurnIndex = new(0);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            Debug.Log("[GameState] Spawned and synced across clients.");
        }
    }

    public ulong CurrentPlayerId =>
        turnOrder.Count > 0 ? turnOrder[currentTurnIndex.Value] : ulong.MaxValue;

    /// <summary>
    /// Called by MainGameManager when ending a turn.
    /// </summary>
    public void AdvanceTurn()
    {
        if (!IsServer) return;

        // Move to next player
        for (int i = 0; i < turnOrder.Count; i++)
        {
            currentTurnIndex.Value = (currentTurnIndex.Value + 1) % turnOrder.Count;
            var nextPlayer = FindPlayerNetwork(CurrentPlayerId);

            if (nextPlayer == null) continue;

            // Skip stunned players
            if (nextPlayer.isStunned.Value)
            {
                Debug.Log($"[Turn] Player {nextPlayer.OwnerClientId} is stunned. Skipping turn.");
                nextPlayer.isStunned.Value = false; // clear stun after skip
                continue;
            }

            // Found valid player
            break;
        }

        Debug.Log($"[Turn] Now it's player {CurrentPlayerId}'s turn.");
    }

    private PlayerNetwork FindPlayerNetwork(ulong clientId)
    {
        foreach (var pn in FindObjectsByType<PlayerNetwork>(FindObjectsSortMode.None))
            if (pn.OwnerClientId == clientId)
                return pn;
        return null;
    }
}
