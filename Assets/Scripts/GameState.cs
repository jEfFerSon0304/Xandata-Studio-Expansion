using UnityEngine;
using Unity.Netcode;

public class GameState : NetworkBehaviour
{
    public static GameState Instance;

    // ✅ MUST NOT BE NULL
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
        if (!IsServer && turnOrder == null)
            turnOrder = new NetworkList<ulong>();
    }


    public ulong CurrentPlayerId =>
        (turnOrder != null && turnOrder.Count > 0)
            ? turnOrder[currentTurnIndex.Value]
            : 999;
}
