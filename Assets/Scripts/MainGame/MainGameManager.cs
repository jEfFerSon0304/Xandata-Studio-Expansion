using UnityEngine;
using Unity.Netcode;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Linq;

public class MainGameManager : NetworkBehaviour
{
    [Header("UI")]
    public TMP_Text headerText;
    public TMP_Text energyText;
    public TMP_Text feedbackText;
    public Transform handPanel;
    public GameObject skillCardPrefab;
    public Button endTurnButton;

    [Header("Turn Order UI")]
    public Button turnOrderButton;
    public TurnOrderPopupUI turnOrderPopupUI;


    [Header("Popups & Targeting")]
    public TargetSelectionUI targetSelectionUI;
    public GameObject attackPopupPrefab;

    private CharacterDataSO myCharacter;
    private int energy = 5;
    private bool initialized = false;

    public override void OnNetworkSpawn()
    {
        StartCoroutine(InitializeWhenReady());
    }

    private IEnumerator InitializeWhenReady()
    {
        while (GameDatabase.Instance == null ||
               PlayerNetwork.LocalPlayer == null ||
               PlayerNetwork.LocalPlayer.SelectedCharacterIndex.Value < 0 ||
               GameState.Instance == null ||
               GameState.Instance.turnOrder.Count == 0)
        {
            yield return null;
        }

        yield return new WaitForSeconds(0.2f);

        LoadMyCharacter();
        ApplyTheme();
        SpawnSkillCards();
        UpdateUI();

        // 🟢 Request turn order and character sync if client
        if (!IsServer && GameState.Instance != null)
        {
            Debug.Log("[MainGameManager] Requesting initial turn order and character sync...");
            GameState.Instance.RequestInitialTurnOrderServerRpc();

            var myPlayer = PlayerNetwork.LocalPlayer;
            if (myPlayer != null)
                myPlayer.RequestCharacterSyncServerRpc();
        }

        // 🔘 Setup Turn Order Button
        if (turnOrderButton != null && turnOrderPopupUI != null)
        {
            turnOrderButton.onClick.RemoveAllListeners();
            turnOrderButton.onClick.AddListener(() =>
            {
                turnOrderPopupUI.TogglePanel();
            });
        }


        initialized = true;
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
    }

    // ============================================================
    // ⚔️ SKILL SYSTEM (RESTORED)
    // ============================================================

    public void TryUseSkill(CharacterDataSO.SkillData skill)
    {
        Debug.Log($"[Skill] Trying to use: {skill.skillName} | Requires target: {skill.requiresTarget}");

        // ⚡ Energy check
        if (skill.energyCost > energy)
        {
            feedbackText.text = "⚡ Not enough energy!";
            return;
        }

        // 🎯 If requires target → open popup (local only!)
        if (skill.requiresTarget)
        {
            targetSelectionUI.gameObject.SetActive(true);
            targetSelectionUI.Open(this, skill);
            return;
        }

        // ⚡ Otherwise execute immediately
        ExecuteSkill(skill, ulong.MaxValue);
    }

    public void OnTargetSelected(CharacterDataSO.SkillData skill, ulong targetClientId)
    {
        energy -= skill.energyCost;
        UpdateUI();

        RequestSkillUseServerRpc(skill.skillName, targetClientId);
    }

    public void ExecuteSkill(CharacterDataSO.SkillData skill, ulong targetClientId)
    {
        energy -= skill.energyCost;
        UpdateUI();

        RequestSkillUseServerRpc(skill.skillName, targetClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    void RequestSkillUseServerRpc(string skillName, ulong targetClientId, ServerRpcParams rpcParams = default)
    {
        ulong senderId = rpcParams.Receive.SenderClientId;
        string attackerName = GameDatabase.Instance.GetCharacterName(senderId);

        // 💬 Notify target
        if (targetClientId != ulong.MaxValue)
            NotifyAttackClientRpc(targetClientId, attackerName, skillName);

        // 🧠 Example: Carabao Ground Slam (stun)
        if (skillName == "Ground Slam" && targetClientId != ulong.MaxValue)
        {
            var target = FindPlayerNetwork(targetClientId);
            if (target != null)
            {
                target.isStunned.Value = true;
                Debug.Log($"[Effect] Player {targetClientId} stunned by Ground Slam!");
            }
        }
    }

    [ClientRpc]
    void NotifyAttackClientRpc(ulong targetClientId, string attackerName, string skillName)
    {
        if (NetworkManager.Singleton.LocalClientId == targetClientId)
        {
            feedbackText.text = $"💥 {attackerName} used {skillName} on you!";

            if (attackPopupPrefab != null)
            {
                var popup = Instantiate(attackPopupPrefab, transform);
                popup.GetComponentInChildren<TMP_Text>().text = $"{attackerName} attacked with {skillName}!";
                Destroy(popup, 3f);
            }
        }
    }

    private PlayerNetwork FindPlayerNetwork(ulong clientId)
    {
        foreach (var pn in FindObjectsByType<PlayerNetwork>(FindObjectsSortMode.None))
            if (pn.OwnerClientId == clientId)
                return pn;
        return null;
    }

    // ============================================================
    // 🕹️ END TURN SYSTEM (unchanged)
    // ============================================================

    public void EndTurn()
    {
        if (!IsServer)
        {
            RequestEndTurnServerRpc();
            return;
        }

        HandleNextTurn();
    }

    [ServerRpc(RequireOwnership = false)]
    void RequestEndTurnServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong senderId = rpcParams.Receive.SenderClientId;
        if (GameState.Instance.CurrentPlayerId != senderId)
        {
            SendFeedbackClientRpc(senderId, "⛔ Not your turn!");
            return;
        }

        HandleNextTurn();
    }

    private void HandleNextTurn()
    {
        var gs = GameState.Instance;
        if (gs == null || gs.turnOrder.Count == 0)
            return;

        gs.AdvanceTurn(); // ✅ skip stunned logic handled here

        // ✅ Full round = +1 energy for everyone
        if (gs.currentTurnIndex.Value == 0)
        {
            foreach (var clientId in gs.turnOrder)
                RestoreEnergyClientRpc(clientId);
        }

        UpdateAllClientsUIClientRpc();
    }

    [ClientRpc]
    void RestoreEnergyClientRpc(ulong targetClientId)
    {
        if (NetworkManager.Singleton.LocalClientId == targetClientId)
        {
            energy = Mathf.Min(5, energy + 1);
            energyText.text = $"Energy: {energy}/5";
            feedbackText.text = "⚡ +1 Energy!";
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
