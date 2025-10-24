using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class DiceManager : NetworkBehaviour
{
    [Header("UI")]
    public Button rollButton;
    public TMP_Text infoText;

    [Header("Dice Settings")]
    public GameObject dicePrefab;
    public Transform spawnPoint;

    private DiceRoller myDice;
    private static Dictionary<ulong, int> playerRolls = new();


    public override void OnNetworkSpawn()
    {
        Debug.Log($"[DiceManager] OnNetworkSpawn() called on {(IsServer ? "Server" : "Client")}");

        if (IsServer)
        {
            playerRolls.Clear();
            int index = 0;

            foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                Vector3 offset = new Vector3(index * 2f - 2f, 1.5f, 0f);
                GameObject dice = Instantiate(dicePrefab, spawnPoint.position + offset, Random.rotation);
                var netObj = dice.GetComponent<NetworkObject>();
                netObj.SpawnWithOwnership(clientId); // 👈 critical line
                Debug.Log($"[Server] Spawned dice for player {clientId}");
                index++;
                Debug.Log($"[Server] Spawned {index} dice objects in scene {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
            }
        }

        Invoke(nameof(FindLocalDice), 1f);

        Debug.Log($"[DiceManager] Running on {(IsServer ? "Server" : "Client")} | Prefab={dicePrefab?.name ?? "NULL"} | PrefabsCount={NetworkManager.Singleton.NetworkConfig.Prefabs.Prefabs.Count}");
        foreach (var p in NetworkManager.Singleton.NetworkConfig.Prefabs.Prefabs)
        {
            Debug.Log($"Prefab in table: {p.Prefab.name}");
        }

    }


    private void FindLocalDice()
    {
        var all = FindObjectsByType<DiceRoller>(FindObjectsSortMode.None);
        myDice = null;

        foreach (var d in all)
        {
            if (d.OwnerClientId == NetworkManager.Singleton.LocalClientId)
            {
                myDice = d;
                SetDiceVisibility(d.gameObject, true);
            }
            else
            {
                SetDiceVisibility(d.gameObject, false);
            }
        }

        if (myDice != null)
        {
            rollButton.interactable = true;
            infoText.text = "Press ROLL to start!";
        }
        else
        {
            rollButton.interactable = false;
            infoText.text = "Waiting for your dice...";
        }
    }

    void SetDiceVisibility(GameObject dice, bool visible)
    {
        // Keep object active — only hide visuals
        var renderers = dice.GetComponentsInChildren<Renderer>(true);
        foreach (var r in renderers) r.enabled = visible;

        var colliders = dice.GetComponentsInChildren<Collider>(true);
        foreach (var c in colliders) c.enabled = visible;
    }

    void OnEnable()
    {
        if (rollButton != null)
            rollButton.onClick.AddListener(OnRollClicked);
    }

    void OnDisable()
    {
        if (rollButton != null)
            rollButton.onClick.RemoveListener(OnRollClicked);
    }

    void OnRollClicked()
    {
        if (myDice == null) return;
        myDice.RollDiceServerRpc();
        rollButton.interactable = false;
        infoText.text = "Rolling...";
    }

    [ServerRpc(RequireOwnership = false)]
    public void ReportDiceResultServerRpc(int value, ulong senderId)
    {
        if (!IsServer) return;

        if (!playerRolls.ContainsKey(senderId))
        {
            playerRolls.Add(senderId, value);
            Debug.Log($"Player {senderId + 1} rolled {value}");
        }

        if (playerRolls.Count == NetworkManager.Singleton.ConnectedClients.Count)
        {
            ResolveTurnOrder();
        }
    }

    void ResolveTurnOrder()
    {
        var ordered = playerRolls.OrderByDescending(kv => kv.Value).ToList();
        string text = "Turn Order:\n";

        int rank = 1;
        foreach (var kv in ordered)
            text += $"{rank++}. Player {kv.Key + 1} → {kv.Value}\n";

        UpdateTurnOrderClientRpc(text);
    }

    [ClientRpc]
    void UpdateTurnOrderClientRpc(string text)
    {
        infoText.text = text;
    }

    [ClientRpc]
    void SetDiceOwnerClientRpc(ulong diceId, ulong ownerId)
    {
        var allDice = FindObjectsByType<NetworkObject>(FindObjectsSortMode.None);
        foreach (var obj in allDice)
        {
            if (obj.NetworkObjectId == diceId)
            {
                var dice = obj.GetComponent<DiceRoller>();
                if (dice != null && ownerId == NetworkManager.Singleton.LocalClientId)
                {
                    // this is my dice, make it visible
                    SetDiceVisibility(dice.gameObject, true);
                }
                else
                {
                    // hide others' dice
                    SetDiceVisibility(dice.gameObject, false);
                }
            }
        }
    }

}
