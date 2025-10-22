using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

public class CharacterSelectionHandler : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text characterNameText;
    public Image characterPortrait;
    public Button prevButton;
    public Button nextButton;

    [Header("Data")]
    public CharacterDataSO[] allCharacters;

    private int selectedIndex = 0;

    void Start()
    {
        prevButton.onClick.AddListener(OnPrev);
        nextButton.onClick.AddListener(OnNext);

        InvokeRepeating(nameof(RefreshFromNetwork), 0.3f, 0.3f);
        UpdateUI();
    }

    void OnPrev()
    {
        selectedIndex = (selectedIndex - 1 + allCharacters.Length) % allCharacters.Length;
        SendSelectionToServer();
    }

    void OnNext()
    {
        selectedIndex = (selectedIndex + 1) % allCharacters.Length;
        SendSelectionToServer();
    }

    void SendSelectionToServer()
    {
        if (PlayerNetwork.LocalPlayer != null)
            PlayerNetwork.LocalPlayer.SetCharacterIndexServerRpc(selectedIndex);
    }

    void RefreshFromNetwork()
    {
        if (PlayerNetwork.LocalPlayer == null) return;

        int netIndex = PlayerNetwork.LocalPlayer.SelectedCharacterIndex.Value;
        if (netIndex != selectedIndex && netIndex >= 0 && netIndex < allCharacters.Length)
        {
            selectedIndex = netIndex;
            UpdateUI();
        }
    }

    void UpdateUI()
    {
        var data = allCharacters[selectedIndex];
        characterNameText.text = data.characterName;
        characterPortrait.sprite = data.portrait;
    }
}
