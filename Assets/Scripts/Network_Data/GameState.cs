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

    [ServerRpc(RequireOwnership = false)]
    public void AddTrophyServerRpc(ulong clientId)
    {
        if (!trophies.ContainsKey(clientId))
            trophies[clientId] = 0;

        trophies[clientId]++;
        MovePlayerToLast(clientId);

        Debug.Log($"[Trophy] Player {clientId} gained a trophy! Total: {trophies[clientId]}");

        // 🏆 Check if this player reached 3 stars
        if (trophies[clientId] >= 3)
        {
            Debug.Log($"[WIN] Player {clientId} reached 3 stars — everyone goes to VictoryScene!");
            StartCoroutine(HandleGameOverForAll(clientId));
            return;
        }

        StartCoroutine(SendSyncDelayed());
    }

    private IEnumerator HandleGameOverForAll(ulong winnerClientId)
    {
        yield return new WaitForSeconds(1f); // small delay for smoothness

        // Optionally save the winner info for the next scene
        PlayerPrefs.SetString("WinnerClientId", winnerClientId.ToString());

        // 🧠 Send the winner to all clients so they know who won
        LoadVictorySceneForAllClientRpc(winnerClientId);
    }

    [ClientRpc]
    private void LoadVictorySceneForAllClientRpc(ulong winnerClientId)
    {
        string winnerName = GameDatabase.Instance != null
            ? GameDatabase.Instance.GetCharacterName(winnerClientId)
            : $"Player {winnerClientId}";

        Debug.Log($"[Victory] {winnerName} reached 3 stars — loading Victory Scene for all players!");

        // Save winner name for the next scene display
        PlayerPrefs.SetString("WinnerName", winnerName);

        // 🏁 Load the victory scene (all clients)
        UnityEngine.SceneManagement.SceneManager.LoadScene("VictoryScene");
    }




    [ClientRpc]
    private void SendPlayerToVictoryClientRpc(ulong clientId, ClientRpcParams rpcParams = default)
    {
        ulong localId = NetworkManager.Singleton.LocalClientId;

        // ✅ Only run for the target player
        if (localId == clientId)
        {
            Debug.Log("[Victory] You reached 3 stars! Loading Victory Scene...");
            UnityEngine.SceneManagement.SceneManager.LoadScene("VictoryScene");
        }
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

    [ClientRpc]
    private void SyncTurnOrderClientRpc(TurnState[] data)
    {
        cachedTurnOrder.Clear();
        foreach (var t in data)
            cachedTurnOrder.Add(t.clientId);

        trophies.Clear();
        foreach (var t in data)
            trophies[t.clientId] = t.trophyCount;

        Debug.Log($"[SyncTurnOrderClientRpc] Updated cached turn order: {string.Join(", ", cachedTurnOrder)}");
        NotifyTurnOrderChanged();
    }

    public IEnumerable<ulong> GetCurrentTurnOrder()
    {
        if (IsServer)
        {
            List<ulong> result = new List<ulong>();
            for (int i = 0; i < turnOrder.Count; i++)
                result.Add(turnOrder[i]);
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

    // 🟢 NEW: Client request + sync
    [ServerRpc(RequireOwnership = false)]
    public void RequestInitialTurnOrderServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong senderId = rpcParams.Receive.SenderClientId;
        Debug.Log($"[GameState] Client {senderId} requested initial turn order.");

        var data = BuildTurnStateArray();
        SyncTurnOrderToClientRpc(data, new ClientRpcParams
        {
            Send = new ClientRpcSendParams { TargetClientIds = new[] { senderId } }
        });
    }

    [ClientRpc]
    private void SyncTurnOrderToClientRpc(TurnState[] data, ClientRpcParams rpcParams = default)
    {
        cachedTurnOrder.Clear();
        foreach (var t in data)
            cachedTurnOrder.Add(t.clientId);

        trophies.Clear();
        foreach (var t in data)
            trophies[t.clientId] = t.trophyCount;

        Debug.Log("[GameState] Initial turn order sync received from server.");
        NotifyTurnOrderChanged();
    }

    public static void NotifyTurnOrderChanged()
    {
        OnTurnOrderChangedEvent?.Invoke();
    }
}
