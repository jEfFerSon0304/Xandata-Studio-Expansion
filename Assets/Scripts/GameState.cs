using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class GameState : NetworkBehaviour
{
    public static GameState Instance;

    public NetworkList<ulong> turnOrder;
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

        // Ensure turnOrder list always exists
        if (turnOrder == null)
            turnOrder = new NetworkList<ulong>();
    }

    public override void OnNetworkSpawn()
    {
        // Server initializes authoritative list
        if (IsServer)
        {
            if (turnOrder == null)
                turnOrder = new NetworkList<ulong>();
        }
    }

    public ulong CurrentPlayerId =>
        turnOrder != null && turnOrder.Count > 0 ? turnOrder[currentTurnIndex.Value] : 999;
}
