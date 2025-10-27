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
        // Wait for all critical systems to sync across network
        while (GameDatabase.Instance == null ||
               PlayerNetwork.LocalPlayer == null ||
               PlayerNetwork.LocalPlayer.SelectedCharacterIndex.Value < 0 ||
               GameState.Instance == null)
        {
            yield return null;
        }

        // ✅ Now safe to continue
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
        endTurnButton.gameObject.SetActive(myTurn);
    }

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

    public void EndTurn()
    {
        if (!IsServer) return;

        var gs = GameState.Instance;
        if (gs == null || gs.turnOrder.Count == 0)
            return;

        // Advance turn index
        gs.currentTurnIndex.Value = (gs.currentTurnIndex.Value + 1) % gs.turnOrder.Count;

        // Restore host energy every new round cycle
        if (gs.currentTurnIndex.Value == 0)
            energy = Mathf.Min(5, energy + 1);

        Debug.Log($"[Turn] Next player: {gs.CurrentPlayerId}");
        UpdateAllClientsUIClientRpc();
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

        endTurnButton.gameObject.SetActive(myTurn);
    }


    void Update()
    {
        if (initialized)
            UpdateUI();
    }
}
