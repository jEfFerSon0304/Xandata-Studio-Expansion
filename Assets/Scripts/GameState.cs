using UnityEngine;
using Unity.Netcode;

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
}
