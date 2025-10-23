using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using System.Linq;

public class CharacterSelectionHandler : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text characterNameText;
    public Image characterPortrait;
    public Button prevButton;
    public Button nextButton;

    [Header("Character Data")]
    public CharacterDataSO[] allCharacters;

    private int selectedIndex = 0;

    void Start()
    {
        prevButton.onClick.AddListener(OnPrev);
        nextButton.onClick.AddListener(OnNext);

        PlayerNetwork.OnAnyReadyStateChanged += RefreshUI;
        InvokeRepeating(nameof(RefreshUI), 0.3f, 0.3f);

        RefreshUI();
    }

    void OnDestroy()
    {
        PlayerNetwork.OnAnyReadyStateChanged -= RefreshUI;
    }

    void OnPrev()
    {
        if (PlayerNetwork.LocalPlayer == null || PlayerNetwork.LocalPlayer.IsReady.Value)
            return; // can't change when locked

        int newIndex = (selectedIndex - 1 + allCharacters.Length) % allCharacters.Length;
        TryChangeCharacter(newIndex);
    }

    void OnNext()
    {
        if (PlayerNetwork.LocalPlayer == null || PlayerNetwork.LocalPlayer.IsReady.Value)
            return;

        int newIndex = (selectedIndex + 1) % allCharacters.Length;
        TryChangeCharacter(newIndex);
    }

    void TryChangeCharacter(int newIndex)
    {
        // Don’t allow selecting locked characters
        var players = FindObjectsOfType<PlayerNetwork>();
        bool locked = players.Any(p => p.IsReady.Value && p.SelectedCharacterIndex.Value == newIndex);
        if (locked)
        {
            Debug.Log($"⚠ Character index {newIndex} is already locked by another player.");
            return;
        }

        PlayerNetwork.LocalPlayer.SetCharacterIndexServerRpc(newIndex);
    }

    void RefreshUI()
    {
        if (PlayerNetwork.LocalPlayer == null) return;

        selectedIndex = PlayerNetwork.LocalPlayer.SelectedCharacterIndex.Value;

        if (selectedIndex < 0 || selectedIndex >= allCharacters.Length)
            selectedIndex = 0;

        var data = allCharacters[selectedIndex];
        characterNameText.text = data.characterName;
        characterPortrait.sprite = data.portrait;

        bool locked = PlayerNetwork.LocalPlayer.IsReady.Value;
        prevButton.interactable = !locked;
        nextButton.interactable = !locked;
    }
}
