using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using TMPro;

public class DiceManager : NetworkBehaviour
{
    [Header("UI References")]
    public Button rollButton;
    public TMP_Text infoText;

    [Header("Dice Setup")]
    public GameObject dicePrefab;
    public Transform spawnPoint;

    private DiceRoller myDice;
    private static readonly Dictionary<ulong, int> playerRollResults = new();

    void Start()
    {
        rollButton.onClick.AddListener(OnRollClicked);
        infoText.text = "🎲 Waiting for players...";
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            playerRollResults.Clear();

            // Spawn one dice for each connected client
            foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                SpawnDiceForClient(clientId);
            }
        }
    }

    private void SpawnDiceForClient(ulong clientId)
    {
        if (dicePrefab == null || spawnPoint == null)
        {
            Debug.LogError("❌ DiceManager: Missing prefab or spawnPoint reference!");
            return;
        }

        // Prevent duplicate spawn
        var existing = FindObjectsByType<DiceRoller>(FindObjectsSortMode.None);
        if (existing.Any(d => d.OwnerClientId == clientId))
            return;

        Vector3 spawnPos = spawnPoint.position + Vector3.right * (clientId * 2f);
        GameObject dice = Instantiate(dicePrefab, spawnPos, Quaternion.identity);

        if (!dice.TryGetComponent(out NetworkObject netObj))
        {
            Debug.LogError("❌ Dice prefab missing NetworkObject component!");
            return;
        }

        netObj.SpawnWithOwnership(clientId);
        Debug.Log($"✅ Spawned dice for Player {clientId}");
    }

    private void OnRollClicked()
    {
        if (myDice == null)
            myDice = FindMyDice();

        if (myDice != null)
        {
            myDice.RequestRoll(); // Let the server roll it
            rollButton.interactable = false;
            infoText.text = "🎲 Rolling...";
            Invoke(nameof(SendResultToServer), 3f);
        }
        else
        {
            Debug.LogWarning("⚠️ No dice found for this player!");
        }
    }

    private DiceRoller FindMyDice()
    {
        var all = FindObjectsByType<DiceRoller>(FindObjectsSortMode.None);
        foreach (var d in all)
        {
            if (d.OwnerClientId == NetworkManager.Singleton.LocalClientId)
                return d;
        }
        return null;
    }

    private void SendResultToServer()
    {
        int result = myDice != null ? myDice.GetValue() : Random.Range(1, 7);
        SendDiceResultServerRpc(result);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SendDiceResultServerRpc(int value, ServerRpcParams rpc = default)
    {
        ulong sender = rpc.Receive.SenderClientId;

        if (!playerRollResults.ContainsKey(sender))
            playerRollResults.Add(sender, value);

        Debug.Log($"[Server] Player {sender} rolled {value}");

        if (playerRollResults.Count == NetworkManager.Singleton.ConnectedClients.Count)
        {
            HandleTurnOrder();
        }
    }

    private void HandleTurnOrder()
    {
        // Ensure unique dice results
        var usedValues = new HashSet<int>();
        foreach (var key in playerRollResults.Keys.ToList())
        {
            int value = playerRollResults[key];
            while (usedValues.Contains(value))
                value = Random.Range(1, 7);
            usedValues.Add(value);
            playerRollResults[key] = value;
        }

        // Sort by highest roll
        var orderedResults = playerRollResults
            .OrderByDescending(kv => kv.Value)
            .ToDictionary(kv => kv.Key, kv => kv.Value);

        string orderText = "🎯 Turn Order:\n";
        int rank = 1;
        foreach (var kv in orderedResults)
        {
            orderText += $"{rank++}. Player {kv.Key} 🎲 {kv.Value}\n";
        }

        Debug.Log(orderText);
        UpdateTurnOrderClientRpc(orderText);

        Invoke(nameof(LoadMainGameScene), 4f);
    }

    [ClientRpc]
    private void UpdateTurnOrderClientRpc(string order)
    {
        infoText.text = order;
    }

    private void LoadMainGameScene()
    {
        if (IsServer)
            NetworkManager.Singleton.SceneManager.LoadScene("MainGame", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
}
