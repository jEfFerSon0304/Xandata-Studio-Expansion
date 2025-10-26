using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;
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

    // Server-authoritative state
    private readonly Dictionary<ulong, int> results = new();       // last roll per player
    private readonly Dictionary<ulong, int> finalRanks = new();    // rank per player (1..N)
    private readonly Dictionary<ulong, Color> playerColors = new();// color per player

    private int TotalPlayers => NetworkManager.Singleton.ConnectedClientsIds.Count;

    // ---------- Lifecycle ----------
    public override void OnNetworkSpawn()
    {
        // Host/Server setup
        rollButton.gameObject.SetActive(IsServer);
        rollButton.onClick.AddListener(OnHostRollPressed);

        if (IsServer)
        {
            // First time server spawns: create dice and sync initial colors/state
            SpawnDiceForAll();
            PushFullSyncToAll();
        }
        else
        {
            // Clients request the latest state when they spawn this scene object
            RequestFullSyncServerRpc();
        }

        UpdateLocalUI(); // safe no-op if data not yet here
    }

    // ---------- Host-only actions ----------
    private void OnHostRollPressed()
    {
        if (!IsServer) return;

        rollButton.interactable = false;
        results.Clear(); // new round
        UpdateLocalUI();

        // Roll all active dice (everyone watches host physics)
        foreach (var dice in FindObjectsByType<DiceRoller>(FindObjectsSortMode.None))
            dice.RollDiceServerRpc();
    }

    private void SpawnDiceForAll()
    {
        Color[] palette = { Color.red, Color.blue, Color.green, Color.yellow };
        int idx = 0;

        foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            Vector3 offset = Vector3.right * (idx * 2f);
            var dice = Instantiate(dicePrefab, spawnPoint.position + offset, spawnPoint.rotation);
            var no = dice.GetComponent<NetworkObject>();
            no.SpawnWithOwnership(clientId);

            var color = palette[idx % palette.Length];
            playerColors[clientId] = color;

            var rend = dice.GetComponentInChildren<Renderer>();
            if (rend) rend.material.color = color;

            idx++;
        }
    }

    // Called by DiceRoller on server when a value is determined
    [ServerRpc(RequireOwnership = false)]
    public void ReportResultServerRpc(int value, ulong playerId)
    {
        results[playerId] = value;
        if (results.Count == TotalPlayers)
        {
            // Simple order: highest to lowest (ties get next rank positions in order received)
            var ordered = results.OrderByDescending(kv => kv.Value).ToList();
            finalRanks.Clear();
            for (int i = 0; i < ordered.Count; i++)
                finalRanks[ordered[i].Key] = i + 1;

            rollButton.interactable = true;
        }
        PushFullSyncToAll();
    }

    // ---------- Sync helpers ----------
    [ServerRpc(RequireOwnership = false)]
    private void RequestFullSyncServerRpc(ServerRpcParams rpc = default)
    {
        PushFullSyncToAll(); // just broadcast current truth
    }

    private void PushFullSyncToAll()
    {
        // pack ranks
        var rankStates = finalRanks.Select(kv => new RankState
        {
            playerId = kv.Key,
            rank = kv.Value
        }).ToArray();

        // pack results
        var resultStates = results.Select(kv => new ResultState
        {
            playerId = kv.Key,
            value = kv.Value
        }).ToArray();

        // pack colors
        var colorStates = playerColors.Select(kv => new ColorState
        {
            playerId = kv.Key,
            r = kv.Value.r,
            g = kv.Value.g,
            b = kv.Value.b,
            a = kv.Value.a
        }).ToArray();

        ApplyFullSyncClientRpc(rankStates, resultStates, colorStates);
    }

    [ClientRpc]
    private void ApplyFullSyncClientRpc(RankState[] ranks, ResultState[] res, ColorState[] cols)
    {
        // rebuild local mirrors
        finalRanks.Clear();
        results.Clear();

        foreach (var c in cols)
        {
            var col = new Color(c.r, c.g, c.b, c.a);
            playerColors[c.playerId] = col;

            // Try apply to spawned dice (host & clients)
            var dice = FindObjectsByType<DiceRoller>(FindObjectsSortMode.None)
                       .FirstOrDefault(d => d.OwnerClientId == c.playerId);
            if (dice != null)
            {
                var rend = dice.GetComponentInChildren<Renderer>();
                if (rend) rend.material.color = col;
            }
        }

        foreach (var r in res) results[r.playerId] = r.value;
        foreach (var r in ranks) finalRanks[r.playerId] = r.rank;

        UpdateLocalUI();
    }

    // ---------- UI ----------
    private void UpdateLocalUI()
    {
        ulong me = NetworkManager.Singleton.LocalClientId;

        // Info text (personal + status)
        string myColor = playerColors.ContainsKey(me) ? ColorToName(playerColors[me]) : "N/A";
        infoText.text = $"Your Dice: {myColor}\n";

        if (finalRanks.TryGetValue(me, out int myRank))
            infoText.text += $"Locked Rank: {myRank}\n";
        else if (results.ContainsKey(me))
            infoText.text += "Waiting for others...\n";
        else
            infoText.text += IsServer ? "Press ROLL!" : "Waiting for host...\n";

        // Order text (global)
        if (finalRanks.Count > 0)
        {
            orderText.text = string.Join("\n",
                finalRanks.OrderBy(kv => kv.Value).Select(kv =>
                {
                    string cname = playerColors.ContainsKey(kv.Key) ? ColorToName(playerColors[kv.Key]) : "N/A";
                    return $"Rank {kv.Value}: Player {kv.Key + 1} ({cname})";
                }));
        }
        else
        {
            orderText.text = "";
        }

        // Host-only roll button
        rollButton.gameObject.SetActive(IsServer);
        if (IsServer && finalRanks.Count == 0)
            rollButton.interactable = true;
    }

    private static string ColorToName(Color c)
    {
        if (c == Color.red) return "RED";
        if (c == Color.blue) return "BLUE";
        if (c == Color.green) return "GREEN";
        if (c == Color.yellow) return "YELLOW";
        return "UNKNOWN";
    }

    // ---------- Net-serializable state ----------
    public struct RankState : INetworkSerializable
    {
        public ulong playerId;
        public int rank;
        public void NetworkSerialize<T>(BufferSerializer<T> s) where T : IReaderWriter
        {
            s.SerializeValue(ref playerId);
            s.SerializeValue(ref rank);
        }
    }

    public struct ResultState : INetworkSerializable
    {
        public ulong playerId;
        public int value;
        public void NetworkSerialize<T>(BufferSerializer<T> s) where T : IReaderWriter
        {
            s.SerializeValue(ref playerId);
            s.SerializeValue(ref value);
        }
    }

    public struct ColorState : INetworkSerializable
    {
        public ulong playerId;
        public float r, g, b, a;
        public void NetworkSerialize<T>(BufferSerializer<T> s) where T : IReaderWriter
        {
            s.SerializeValue(ref playerId);
            s.SerializeValue(ref r);
            s.SerializeValue(ref g);
            s.SerializeValue(ref b);
            s.SerializeValue(ref a);
        }
    }
}
