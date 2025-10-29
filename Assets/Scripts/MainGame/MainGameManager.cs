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


    void RefreshTurnOrderUI()
    {
        if (GameState.Instance == null || turnOrderPanel == null) return;

        foreach (Transform child in turnOrderPanel)
            Destroy(child.gameObject);

        foreach (var clientId in GameState.Instance.turnOrder)
        {
            var entry = Instantiate(turnOrderEntryPrefab, turnOrderPanel);
            var nameText = entry.transform.Find("NameText").GetComponent<TMP_Text>();
            var trophyText = entry.transform.Find("TrophyText").GetComponent<TMP_Text>();

            string playerName = GameDatabase.Instance.GetCharacterName(clientId);
            int trophyCount = GameState.Instance.GetTrophyCount(clientId);

            nameText.text = playerName;
            trophyText.text = $"🏆 {trophyCount}";

            if (GameState.Instance.CurrentPlayerId == clientId)
                nameText.color = Color.yellow;
            else
                nameText.color = Color.white;
        }
    }

    // (The rest of your MainGameManager code is unchanged)
}
