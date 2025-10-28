using UnityEngine;
using Unity.Netcode;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class MainGameManager : NetworkBehaviour
{
    [Header("UI")]
    public TMP_Text headerText;
    public TMP_Text energyText;
    public TMP_Text feedbackText;
    public Transform handPanel;
    public GameObject skillCardPrefab;
    public Button endTurnButton;

    private CharacterDataSO myCharacter;
    private int energy = 5;
    private bool initialized = false;

    public override void OnNetworkSpawn()
    {
        StartCoroutine(InitializeWhenReady());
    }

    private IEnumerator InitializeWhenReady()
    {
        // Wait for all systems to be ready across network
        while (GameDatabase.Instance == null ||
               PlayerNetwork.LocalPlayer == null ||
               PlayerNetwork.LocalPlayer.SelectedCharacterIndex.Value < 0 ||
               GameState.Instance == null)
        {
            yield return null;
        }

        // ✅ Safe to continue
        LoadMyCharacter();
        ApplyTheme();
        SpawnSkillCards();
        UpdateUI();

        initialized = true;
        Debug.Log($"[MainGame] Initialized for player {NetworkManager.Singleton.LocalClientId}");
    }

    void LoadMyCharacter()
    {
        int index = PlayerNetwork.LocalPlayer.SelectedCharacterIndex.Value;
        myCharacter = GameDatabase.Instance.allCharacters[index];
    }

    void ApplyTheme()
    {
        headerText.color = myCharacter.themeColor;
        energyText.color = myCharacter.themeColor;
        feedbackText.color = myCharacter.themeColor;
    }

    void SpawnSkillCards()
    {
        foreach (Transform t in handPanel)
            Destroy(t.gameObject);

        foreach (var skill in myCharacter.skills)
        {
            var card = Instantiate(skillCardPrefab, handPanel);
            var ui = card.GetComponent<SkillCardUI>();
            ui.manager = this;
            ui.Setup(skill);
        }
    }

    void UpdateUI()
    {
        if (!initialized) return;

        ulong myId = NetworkManager.Singleton.LocalClientId;
        bool myTurn = (GameState.Instance.CurrentPlayerId == myId);

        headerText.text = myTurn
            ? $"{myCharacter.characterName}'s Turn!"
            : "Waiting for your turn...";

        energyText.text = $"Energy: {energy}/5";
        endTurnButton.interactable = myTurn;
        endTurnButton.gameObject.SetActive(true);
    }

    // ============================================================
    // 🪄 SKILL USAGE
    // ============================================================

    public void TryUseSkill(CharacterDataSO.SkillData skill)
    {
        if (GameState.Instance.CurrentPlayerId != NetworkManager.Singleton.LocalClientId)
        {
            feedbackText.text = "⛔ Not your turn!";
            return;
        }

        if (skill.energyCost > energy)
        {
            feedbackText.text = "⚡ Not enough energy!";
            return;
        }

        energy -= skill.energyCost;
        feedbackText.text = $"✅ {skill.skillName} Activated!";
        UpdateUI();
    }

    // ============================================================
    // 🕹️ END TURN SYSTEM (WORKS FOR BOTH HOST & CLIENT)
    // ============================================================

    public void EndTurn()
    {
        if (!IsServer)
        {
            // 🧠 If not host, ask the server to handle it
            RequestEndTurnServerRpc();
            return;
        }

        // 🧠 Host directly handles the logic
        HandleNextTurn();
    }

    [ServerRpc(RequireOwnership = false)]
    void RequestEndTurnServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong senderId = rpcParams.Receive.SenderClientId;

        if (GameState.Instance == null)
            return;

        // ✅ Only allow the player whose turn it is
        if (GameState.Instance.CurrentPlayerId != senderId)
        {
            SendFeedbackClientRpc(senderId, "⛔ You can’t end another player’s turn!");
            return;
        }

        HandleNextTurn();
    }

    private void HandleNextTurn()
    {
        var gs = GameState.Instance;
        if (gs == null || gs.turnOrder.Count == 0)
            return;

        // ✅ Advance to next turn
        gs.currentTurnIndex.Value = (gs.currentTurnIndex.Value + 1) % gs.turnOrder.Count;

        // ✅ Full round completed? Restore energy to everyone!
        if (gs.currentTurnIndex.Value == 0)
        {
            foreach (var clientId in gs.turnOrder)
            {
                RestoreEnergyClientRpc(clientId);
            }
        }

        Debug.Log($"[Turn] Next player: {gs.CurrentPlayerId}");

        // 🔄 Update everyone’s UI
        UpdateAllClientsUIClientRpc();
    }

    // ============================================================
    // 🔁 CLIENTRPC UPDATES
    // ============================================================

    [ClientRpc]
    void RestoreEnergyClientRpc(ulong targetClientId)
    {
        // ✅ Only affects intended client
        if (NetworkManager.Singleton.LocalClientId == targetClientId)
        {
            energy = Mathf.Min(5, energy + 1);
            energyText.text = $"Energy: {energy}/5";
            feedbackText.text = "⚡ +1 Energy!";
            Debug.Log($"[Energy] +1 Energy for Player {targetClientId}");
        }
    }

    [ClientRpc]
    void UpdateAllClientsUIClientRpc()
    {
        if (GameState.Instance == null) return;

        ulong myId = NetworkManager.Singleton.LocalClientId;
        bool myTurn = (GameState.Instance.CurrentPlayerId == myId);

        headerText.text = myTurn
            ? $"{myCharacter.characterName}'s Turn!"
            : "Waiting for your turn...";

        endTurnButton.interactable = myTurn;
    }

    [ClientRpc]
    void SendFeedbackClientRpc(ulong targetClientId, string message)
    {
        if (NetworkManager.Singleton.LocalClientId == targetClientId)
            feedbackText.text = message;
    }

    void Update()
    {
        if (initialized)
            UpdateUI();
    }
}
