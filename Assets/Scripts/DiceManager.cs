using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
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
    private NetworkVariable<bool> allRolled = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private static Dictionary<ulong, int> playerRollResults = new();

    void Start()
    {
        rollButton.onClick.AddListener(OnRollClicked);
        infoText.text = "🎲 Waiting for all players...";
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            // Spawn each player's dice on their screen
            SpawnMyDiceServerRpc(OwnerClientId);
        }

        if (IsServer)
            playerRollResults.Clear();
    }

    [ServerRpc(RequireOwnership = false)]
    void SpawnMyDiceServerRpc(ulong clientId, ServerRpcParams rpcParams = default)
    {
        GameObject dice = Instantiate(dicePrefab, spawnPoint.position + Vector3.right * clientId * 1.5f, Quaternion.identity);
        var netObj = dice.GetComponent<NetworkObject>();
        netObj.SpawnWithOwnership(clientId);
    }

    void OnRollClicked()
    {
        if (myDice == null)
            myDice = FindMyDice();

        if (myDice != null)
        {
            myDice.RollDice();
            rollButton.interactable = false;
            infoText.text = "🎲 Rolling...";
            Invoke(nameof(SendResultToServer), 3f);
        }
    }

    DiceRoller FindMyDice()
    {
        var all = FindObjectsOfType<DiceRoller>();
        foreach (var d in all)
        {
            if (d.OwnerClientId == NetworkManager.Singleton.LocalClientId)
                return d;
        }
        return null;
    }

    void SendResultToServer()
    {
        int result = myDice != null ? myDice.GetValue() : Random.Range(1, 7);
        SendDiceResultServerRpc(result);
    }

    [ServerRpc(RequireOwnership = false)]
    void SendDiceResultServerRpc(int value, ServerRpcParams rpc = default)
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

    void HandleTurnOrder()
    {
        // Ensure unique results
        var values = playerRollResults.Values.ToList();
        for (int i = 0; i < values.Count; i++)
        {
            for (int j = i + 1; j < values.Count; j++)
            {
                if (values[i] == values[j])
                    values[j] = Random.Range(1, 7); // re-roll duplicate
            }
        }

        playerRollResults = playerRollResults
            .OrderByDescending(kv => kv.Value)
            .ToDictionary(kv => kv.Key, kv => kv.Value);

        string orderText = "Turn Order:\n";
        int rank = 1;
        foreach (var kv in playerRollResults)
        {
            orderText += $"{rank++}. Player {kv.Key} 🎲 {kv.Value}\n";
        }

        Debug.Log(orderText);
        UpdateTurnOrderClientRpc(orderText);

        // Wait then load main scene
        Invoke(nameof(LoadMainGameScene), 4f);
    }

    [ClientRpc]
    void UpdateTurnOrderClientRpc(string order)
    {
        infoText.text = order;
    }

    void LoadMainGameScene()
    {
        if (IsServer)
            NetworkManager.Singleton.SceneManager.LoadScene("MainGame", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
}
