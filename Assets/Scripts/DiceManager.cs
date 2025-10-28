using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(NetworkObject))]
public class DiceManager : NetworkBehaviour
{
    [Header("UI")]
    public TMP_Text infoText;
    public TMP_Text orderText;
    public Button rollButton;

    [Header("Dice")]
    public GameObject dicePrefab;
    public Transform spawnPoint;

    [SerializeField] private GameObject gameStatePrefab;


    private readonly Dictionary<ulong, int> results = new();
    private readonly Dictionary<ulong, int> finalRanks = new();
    private readonly Dictionary<ulong, Color> playerColors = new();
    private readonly HashSet<ulong> tieBand = new();

    private int TotalPlayers => NetworkManager.Singleton.ConnectedClientsIds.Count;

    public override void OnNetworkSpawn()
    {
        rollButton.onClick.RemoveAllListeners();
        rollButton.onClick.AddListener(OnHostRollPressed);
        rollButton.gameObject.SetActive(IsServer);

        if (IsServer)
        {
            SpawnDiceForAll();
            PushSyncToAll();
        }
        else
        {
            RequestSyncServerRpc();
        }

        UpdateLocalUI();
    }

    private void SpawnDiceForAll()
    {
        Color[] palette = { Color.red, Color.blue, Color.green, Color.yellow };
        int i = 0;

        foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            var dice = Instantiate(dicePrefab, spawnPoint.position + Vector3.right * (i * 2f), spawnPoint.rotation);
            dice.GetComponent<NetworkObject>().SpawnWithOwnership(clientId);

            var color = palette[i % palette.Length];
            playerColors[clientId] = color;

            var rend = dice.GetComponentInChildren<Renderer>();
            if (rend) rend.material.color = color;

            i++;
        }
    }

    private HashSet<ulong> GetExpectedRollers()
    {
        if (tieBand.Count > 0) return new HashSet<ulong>(tieBand);

        return new HashSet<ulong>(
            NetworkManager.Singleton.ConnectedClientsIds.Where(id => !finalRanks.ContainsKey(id)));
    }

    private void OnHostRollPressed()
    {
        if (!IsServer) return;
        var expected = GetExpectedRollers();
        if (expected.Count == 0) return;

        foreach (var id in expected) results.Remove(id);

        rollButton.interactable = false;
        UpdateLocalUI();

        foreach (var d in FindObjectsByType<DiceRoller>(FindObjectsSortMode.None))
            if (expected.Contains(d.OwnerClientId))
                d.RollDiceServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public void ReportResultServerRpc(int value, ulong playerId)
    {
        if (finalRanks.ContainsKey(playerId)) return;
        results[playerId] = value;

        var expected = GetExpectedRollers();

        PushSyncToAll();

        if (expected.All(results.ContainsKey))
        {
            EvaluateRound(expected);
        }

        PushSyncToAll();
        UpdateLocalUI();
    }

    private void EvaluateRound(HashSet<ulong> expected)
    {
        tieBand.Clear();

        var groups = expected.GroupBy(id => results[id])
                             .OrderByDescending(g => g.Key)
                             .ToList();

        int nextTop = NextUnusedRankFromTop();
        int nextBottom = NextUnusedRankFromBottom();

        // Highest group
        var highest = groups.First();
        if (highest.Count() == 1) Lock(highest.First(), nextTop);
        else foreach (var id in highest) tieBand.Add(id);

        // Re-check remaining
        expected = GetExpectedRollers();
        if (expected.Count == 0) FinalizeResults();

        groups = expected.GroupBy(id => results[id])
                         .OrderByDescending(g => g.Key)
                         .ToList();

        var lowest = groups.Last();
        if (lowest.Count() == 1) Lock(lowest.First(), nextBottom);
        else foreach (var id in lowest) tieBand.Add(id);

        foreach (var g in groups)
        {
            if (g.Count() == 1)
            {
                var id = g.First();
                if (!finalRanks.ContainsKey(id))
                    Lock(id, NextUnusedRankFromTop());
            }
            else foreach (var id in g) tieBand.Add(id);
        }

        if (finalRanks.Count >= TotalPlayers)
            FinalizeResults();

        rollButton.interactable = tieBand.Count > 0;
    }

    private void Lock(ulong id, int rank)
    {
        if (!finalRanks.ContainsKey(id))
            finalRanks[id] = rank;
    }

    private void FinalizeResults()
    {
        tieBand.Clear();
        PushSyncToAll();
        StartCoroutine(StartMainGame());
    }

    private System.Collections.IEnumerator StartMainGame()
    {
        yield return new WaitForEndOfFrame();
        CleanupDice();
        SaveTurnOrderToGameState();
        if (IsServer)
            NetworkManager.Singleton.SceneManager.LoadScene("MainGame", LoadSceneMode.Single);
    }

    private void CleanupDice()
    {
        if (!IsServer) return;
        foreach (var d in FindObjectsByType<DiceRoller>(FindObjectsSortMode.None))
            d.GetComponent<NetworkObject>().Despawn();
    }

    private void SaveTurnOrderToGameState()
    {
        if (finalRanks.Count == 0)
        {
            Debug.LogWarning("[DiceManager] No ranks to save.");
            return;
        }

        GameState gs = FindFirstObjectByType<GameState>();
        if (gs == null)
        {
            if (gameStatePrefab == null)
            {
                Debug.LogError("[DiceManager] Missing GameState prefab reference!");
                return;
            }

            var go = Instantiate(gameStatePrefab);
            gs = go.GetComponent<GameState>();
            DontDestroyOnLoad(go);

            if (IsServer)
            {
                var netObj = go.GetComponent<NetworkObject>();
                if (!netObj.IsSpawned)
                    netObj.Spawn(true);
            }
        }

        gs.turnOrder.Clear();
        foreach (var kv in finalRanks.OrderBy(x => x.Value))
        {
            gs.turnOrder.Add(kv.Key);
            Debug.Log($"[DiceManager] Saved Player {kv.Key} → Rank {kv.Value}");
        }
    }

    private int NextUnusedRankFromTop()
    {
        for (int i = 1; i <= TotalPlayers; i++)
            if (!finalRanks.ContainsValue(i))
                return i;
        return TotalPlayers;
    }

    private int NextUnusedRankFromBottom()
    {
        for (int i = TotalPlayers; i >= 1; i--)
            if (!finalRanks.ContainsValue(i))
                return i;
        return 1;
    }

    private void PushSyncToAll()
    {
        var payload = BuildStateArray();
        SyncClientRpc(payload);
    }

    private PlayerState[] BuildStateArray()
    {
        return NetworkManager.Singleton.ConnectedClientsIds.Select(id =>
        {
            playerColors.TryGetValue(id, out var col);
            results.TryGetValue(id, out var roll);
            finalRanks.TryGetValue(id, out var rank);

            return new PlayerState
            {
                clientId = id,
                rollValue = roll,
                rank = rank,
                r = col.r,
                g = col.g,
                b = col.b,
                a = col.a
            };
        }).ToArray();
    }

    [ClientRpc]
    private void SyncClientRpc(PlayerState[] payload)
    {
        results.Clear();
        finalRanks.Clear();

        foreach (var p in payload)
        {
            playerColors[p.clientId] = new Color(p.r, p.g, p.b, p.a);
            if (p.rollValue > 0) results[p.clientId] = p.rollValue;
            if (p.rank > 0) finalRanks[p.clientId] = p.rank;

            var dice = FindObjectsByType<DiceRoller>(FindObjectsSortMode.None)
                       .FirstOrDefault(d => d.OwnerClientId == p.clientId);

            if (dice != null)
            {
                var rend = dice.GetComponentInChildren<Renderer>();
                if (rend) rend.material.color = playerColors[p.clientId];
            }
        }

        UpdateLocalUI();
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestSyncServerRpc(ServerRpcParams r = default)
        => PushSyncToAll();

    private void UpdateLocalUI()
    {
        ulong me = NetworkManager.Singleton.LocalClientId;

        infoText.text =
            "Your Dice: " + (playerColors.TryGetValue(me, out var col) ? ColorToName(col) : "N/A") + "\n" +
            (finalRanks.TryGetValue(me, out int rank)
                ? ("Locked Rank: " + rank + "\n")
                : (GetExpectedRollers().Contains(me) ? "Rolling...\n" : "Waiting...\n"));


        if (finalRanks.Count > 0)
        {
            orderText.text = string.Join("\n",
                finalRanks.OrderBy(k => k.Value)
                .Select(k => $"Rank {k.Value}: Player {k.Key} ({ColorToName(playerColors[k.Key])})"));
        }
        else orderText.text = "";

        rollButton.gameObject.SetActive(IsServer);
    }

    private static string ColorToName(Color c) =>
        (c == Color.red ? "Red" :
         c == Color.blue ? "Blue" :
         c == Color.green ? "Green" :
         c == Color.yellow ? "Yellow" : "Unknown");

    public struct PlayerState : INetworkSerializable
    {
        public ulong clientId;
        public int rollValue;
        public int rank;
        public float r, g, b, a;

        public void NetworkSerialize<T>(BufferSerializer<T> s) where T : IReaderWriter
        {
            s.SerializeValue(ref clientId);
            s.SerializeValue(ref rollValue);
            s.SerializeValue(ref rank);
            s.SerializeValue(ref r);
            s.SerializeValue(ref g);
            s.SerializeValue(ref b);
            s.SerializeValue(ref a);
        }
    }
}
