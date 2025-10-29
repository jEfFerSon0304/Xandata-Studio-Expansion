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

    [Header("Turn Order Display")]
    public Transform turnOrderPanel;
    public GameObject turnOrderEntryPrefab;

    [Header("Popups & Targeting")]
    public TargetSelectionUI targetSelectionUI;
    public GameObject attackPopupPrefab;

    private CharacterDataSO myCharacter;
    private int energy = 5;
    private bool initialized = false;

    private void OnEnable() => GameState.OnTurnOrderChangedEvent += RefreshTurnOrderUI;
    private void OnDisable() => GameState.OnTurnOrderChangedEvent -= RefreshTurnOrderUI;

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
        RefreshTurnOrderUI();

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
    // 🧩 TURN ORDER DISPLAY
    // ============================================================
    void RefreshTurnOrderUI()
    {
        // ✅ Safety checks to avoid null/destroyed refs
        if (GameState.Instance == null)
        {
            Debug.LogWarning("[TurnOrderUI] GameState missing, skipping refresh.");
            return;
        }

        if (this == null || gameObject == null)
        {
            Debug.LogWarning("[TurnOrderUI] MainGameManager destroyed, skipping refresh.");
            return;
        }

        if (turnOrderPanel == null || turnOrderEntryPrefab == null)
        {
            Debug.LogWarning("[TurnOrderUI] UI references missing, skipping refresh.");
            return;
        }

        // In case the prefab was destroyed due to scene reload or RPC timing
        if (turnOrderEntryPrefab.Equals(null))
        {
            Debug.LogWarning("[TurnOrderUI] Turn order entry prefab is invalid (destroyed).");
            return;
        }

        // ✅ Destroy old list safely
        foreach (Transform child in turnOrderPanel)
        {
            if (child != null)
                Destroy(child.gameObject);
        }

        // ✅ Rebuild UI
        foreach (var clientId in GameState.Instance.GetCurrentTurnOrder())
        {
            // Skip if prefab was unloaded or invalid
            if (turnOrderEntryPrefab == null) break;

            var entry = Instantiate(turnOrderEntryPrefab, turnOrderPanel);
            if (entry == null) continue;

            TMP_Text nameText = entry.transform.Find("NameText")?.GetComponent<TMP_Text>();
            TMP_Text trophyText = entry.transform.Find("TrophyText")?.GetComponent<TMP_Text>();

            if (nameText == null || trophyText == null)
            {
                Debug.LogWarning("[TurnOrderUI] Missing NameText/TrophyText in prefab.");
                continue;
            }

            string playerName = GameDatabase.Instance?.GetCharacterName(clientId) ?? $"Player {clientId}";
            int trophyCount = GameState.Instance.GetTrophyCount(clientId);

            nameText.text = playerName;
            trophyText.text = $"🏆 {trophyCount}";

            if (GameState.Instance.CurrentPlayerId == clientId)
                nameText.color = Color.yellow;
            else
                nameText.color = Color.white;
        }
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
