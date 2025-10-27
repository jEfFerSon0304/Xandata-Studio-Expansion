using UnityEngine;
using Unity.Netcode;
using TMPro;

public class MainGameManager : NetworkBehaviour
{
    public TMP_Text turnText;
    public TMP_Text energyText;
    public TMP_Text feedbackText;
    public Transform handPanel;
    public GameObject skillCardPrefab;
    public GameObject endTurnButton;

    private CharacterDataSO myCharacter;
    private int energy = 5;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        LoadMyCharacter();
        ApplyTheme();
        SpawnSkillCards();
        UpdateUI();
    }

    void LoadMyCharacter()
    {
        int index = PlayerNetwork.LocalPlayer.SelectedCharacterIndex.Value;
        myCharacter = GameDatabase.Instance.allCharacters[index];
    }

    void ApplyTheme()
    {
        turnText.color = myCharacter.themeColor;
        energyText.color = myCharacter.themeColor;
    }

    void SpawnSkillCards()
    {
        foreach (Transform t in handPanel)
            Destroy(t.gameObject);

        foreach (var skill in myCharacter.skills)
        {
            var cardObj = Instantiate(skillCardPrefab, handPanel);
            var ui = cardObj.GetComponent<SkillCardUI>();
            ui.manager = this;
            ui.Setup(skill);
        }
    }

    void UpdateUI()
    {
        ulong myId = NetworkManager.Singleton.LocalClientId;
        bool myTurn = (GameState.Instance.CurrentPlayerId == myId);

        turnText.text = myTurn
            ? $"{myCharacter.characterName}'s Turn!"
            : "Opponent's Turn";

        energyText.text = $"Energy: {energy}/5";
        endTurnButton.SetActive(myTurn);
    }

    public void TryUseSkill(CharacterDataSO.SkillData skill)
    {
        if (GameState.Instance.CurrentPlayerId != NetworkManager.Singleton.LocalClientId)
        {
            feedbackText.text = "Not your turn!";
            return;
        }

        if (skill.energyCost > energy)
        {
            feedbackText.text = "Not enough energy!";
            return;
        }

        energy -= skill.energyCost;
        feedbackText.text = $"{skill.skillName} Activated!";
        UpdateUI();
    }

    public void EndTurn()
    {
        if (!IsServer) return;

        GameState.Instance.currentTurnIndex.Value =
            (GameState.Instance.currentTurnIndex.Value + 1) % GameState.Instance.turnOrder.Count;

        if (GameState.Instance.currentTurnIndex.Value == 0)
            energy = Mathf.Min(5, energy + 1);

        UpdateAllClientsUIClientRpc();
    }

    [ClientRpc]
    void UpdateAllClientsUIClientRpc()
    {
        UpdateUI();
    }
}
