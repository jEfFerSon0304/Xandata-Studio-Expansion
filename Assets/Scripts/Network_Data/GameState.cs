using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class GameState : NetworkBehaviour
{
    public static GameState Instance;

    public NetworkList<ulong> turnOrder = new NetworkList<ulong>();
    public NetworkVariable<int> currentTurnIndex = new(0);

    // 🏆 Trophy tracking
    private Dictionary<ulong, int> trophies = new();
    private List<ulong> cachedTurnOrder = new();


    public delegate void TurnOrderChanged();
    public static event TurnOrderChanged OnTurnOrderChangedEvent;

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
            Debug.Log("[GameState] Spawned and synced across clients.");
    }

    public ulong CurrentPlayerId =>
        turnOrder.Count > 0 ? turnOrder[currentTurnIndex.Value] : ulong.MaxValue;

    public void AdvanceTurn()
    {
        if (!IsServer) return;

        for (int i = 0; i < turnOrder.Count; i++)
        {
            currentTurnIndex.Value = (currentTurnIndex.Value + 1) % turnOrder.Count;
            var nextPlayer = FindPlayerNetwork(CurrentPlayerId);

            if (nextPlayer == null) continue;

            if (nextPlayer.isStunned.Value)
            {
                Debug.Log($"[Turn] Player {nextPlayer.OwnerClientId} is stunned. Skipping turn.");
                nextPlayer.isStunned.Value = false;
                continue;
            }

            break;
        }

        Debug.Log($"[Turn] Now it's player {CurrentPlayerId}'s turn.");
        SyncTurnOrderClientRpc(BuildTurnStateArray());
    }

    private PlayerNetwork FindPlayerNetwork(ulong clientId)
    {
        foreach (var pn in FindObjectsByType<PlayerNetwork>(FindObjectsSortMode.None))
            if (pn.OwnerClientId == clientId)
                return pn;
        return null;
    }

    // ===============================================================
    // 🏆 TROPHY SYSTEM
    // ===============================================================

    [ServerRpc(RequireOwnership = false)]
    public void AddTrophyServerRpc(ulong clientId)
    {
        if (!trophies.ContainsKey(clientId))
            trophies[clientId] = 0;

        trophies[clientId]++;
        MovePlayerToLast(clientId);
        Debug.Log($"[Trophy] Player {clientId} gained a trophy! Total: {trophies[clientId]}");

        StartCoroutine(SendSyncDelayed()); // 👈 safer sync
    }

    private IEnumerator SendSyncDelayed()
    {
        yield return new WaitForSeconds(0.1f);
        SyncTurnOrderClientRpc(BuildTurnStateArray());
    }


    [ServerRpc(RequireOwnership = false)]
    public void RemoveTrophyServerRpc(ulong clientId)
    {
        if (!trophies.ContainsKey(clientId)) return;

        trophies[clientId] = Mathf.Max(0, trophies[clientId] - 1);
        Debug.Log($"[Trophy] Player {clientId} lost a trophy. Total: {trophies[clientId]}");

        SyncTurnOrderClientRpc(BuildTurnStateArray());
    }

    private void MovePlayerToLast(ulong clientId)
    {
        if (!turnOrder.Contains(clientId)) return;

        turnOrder.Remove(clientId);
        turnOrder.Add(clientId);
        Debug.Log($"[TurnOrder] Player {clientId} moved to last place.");
    }

    // ===============================================================
    // 📡 SYNC TURN ORDER & TROPHIES
    // ===============================================================

    [ClientRpc]
    private void SyncTurnOrderClientRpc(TurnState[] data)
    {
        // 🧩 DO NOT skip host — it’s both server & client
        cachedTurnOrder.Clear();
        foreach (var t in data)
            cachedTurnOrder.Add(t.clientId);

        trophies.Clear();
        foreach (var t in data)
            trophies[t.clientId] = t.trophyCount;

        Debug.Log($"[SyncTurnOrderClientRpc] Updated cached turn order: {string.Join(", ", cachedTurnOrder)}");

        OnTurnOrderChangedEvent?.Invoke();
    }


    public IEnumerable<ulong> GetCurrentTurnOrder()
    {
        if (IsServer)
        {
            // Manually copy items from NetworkList to a normal list
            List<ulong> result = new List<ulong>();
            for (int i = 0; i < turnOrder.Count; i++)
            {
                result.Add(turnOrder[i]);
            }
            return result;
        }
        else
        {
            return cachedTurnOrder;
        }
    }






    private TurnState[] BuildTurnStateArray()
    {
        List<TurnState> states = new List<TurnState>();

        for (int i = 0; i < turnOrder.Count; i++)
        {
            ulong id = turnOrder[i];
            trophies.TryGetValue(id, out int trophyCount);

            states.Add(new TurnState
            {
                clientId = id,
                trophyCount = trophyCount
            });
        }

        return states.ToArray();
    }


    public int GetTrophyCount(ulong clientId)
    {
        return trophies.TryGetValue(clientId, out int count) ? count : 0;
    }

    [System.Serializable]
    public struct TurnState : INetworkSerializable
    {
        public ulong clientId;
        public int trophyCount;

        public void NetworkSerialize<T>(BufferSerializer<T> s) where T : IReaderWriter
        {
            s.SerializeValue(ref clientId);
            s.SerializeValue(ref trophyCount);
        }
    }
}
