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

        InvokeRepeating(nameof(UpdateUIFromNetwork), 0.3f, 0.3f);
        UpdateUI();
    }

    void OnPrev()
    {
        selectedIndex = (selectedIndex - 1 + allCharacters.Length) % allCharacters.Length;
        SendSelection();
    }

    void OnNext()
    {
        selectedIndex = (selectedIndex + 1) % allCharacters.Length;
        SendSelection();
    }

    void SendSelection()
    {
        if (PlayerNetwork.LocalPlayer != null)
        {
            PlayerNetwork.LocalPlayer.SetCharacterIndexServerRpc(selectedIndex);
        }
    }

    void UpdateUI()
    {
        var data = allCharacters[selectedIndex];
        characterNameText.text = data.characterName;
        characterPortrait.sprite = data.portrait;
    }

    void UpdateUIFromNetwork()
    {
        if (PlayerNetwork.LocalPlayer == null) return;
        selectedIndex = PlayerNetwork.LocalPlayer.SelectedCharacterIndex.Value;
        if (selectedIndex >= 0 && selectedIndex < allCharacters.Length)
            UpdateUI();
    }
}
