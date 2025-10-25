using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class DiceManager : NetworkBehaviour
{
    public GameObject dicePrefab;
    public Transform spawnPoint;
    public Button rollButton;
    public TMP_Text infoText;
    public TMP_Text orderText;

    Dictionary<ulong, int> results = new();
    DiceRoller myDice;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            SpawnDiceForAll();
        }

        rollButton.onClick.AddListener(OnRollClicked);
        infoText.text = "Press ROLL!";
    }

    void SpawnDiceForAll()
    {
        foreach (var id in NetworkManager.Singleton.ConnectedClientsIds)
        {
            Vector3 pos = spawnPoint.position + Vector3.right * (id * 2f);
            var obj = Instantiate(dicePrefab, pos, Quaternion.identity);
            obj.GetComponent<NetworkObject>().SpawnWithOwnership(id);
        }
    }

    public void OnRollClicked()
    {
        if (myDice == null)
        {
            myDice = FindObjectsOfType<DiceRoller>()
                .First(x => x.OwnerClientId == NetworkManager.Singleton.LocalClientId);
        }

        rollButton.interactable = false;
        infoText.text = "Rolling...";

        myDice.RollDiceServerRpc();

        Invoke(nameof(SendResult), 3f);
    }

    void SendResult()
    {
        int val = myDice.GetValue();
        SubmitServerRpc(val);
    }

    [ServerRpc(RequireOwnership = false)]
    void SubmitServerRpc(int value, ServerRpcParams rpc = default)
    {
        ulong sender = rpc.Receive.SenderClientId;

        if (!results.ContainsKey(sender))
            results.Add(sender, value);

        // All players rolled?
        if (results.Count == NetworkManager.Singleton.ConnectedClients.Count)
        {
            ProcessResults();
        }
    }

    void ProcessResults()
    {
        var ordered = results
            .OrderByDescending(k => k.Value)
            .ToList();

        string text = "Turn Order:\n";
        int rank = 1;
        foreach (var r in ordered)
        {
            text += $"{rank++}. Player {r.Key} rolled {r.Value}\n";
        }

        UpdateOrderClientRpc(text);
    }

    [ClientRpc]
    void UpdateOrderClientRpc(string text)
    {
        orderText.text = text;
        infoText.text = "Done!";
    }
}
