using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using System.Linq;

public class CharacterSelectionHandler : MonoBehaviour
{
    public TMP_Text nameText;
    public Image portrait;
    public Button prevButton, nextButton;
    public CharacterDataSO[] allCharacters;
    int selectedIndex;

    void Start()
    {
        prevButton.onClick.AddListener(() => ChangeCharacter(-1));
        nextButton.onClick.AddListener(() => ChangeCharacter(1));
        PlayerNetwork.OnAnyStateChanged += Refresh;
        InvokeRepeating(nameof(Refresh), 0.2f, 0.2f);
    }

    void OnDestroy() => PlayerNetwork.OnAnyStateChanged -= Refresh;

    void ChangeCharacter(int dir)
    {
        if (PlayerNetwork.LocalPlayer == null || PlayerNetwork.LocalPlayer.IsReady.Value) return;
        int newIndex = (selectedIndex + dir + allCharacters.Length) % allCharacters.Length;
        PlayerNetwork.LocalPlayer.SetCharacterIndexServerRpc(newIndex);
    }

    void Refresh()
    {
        if (PlayerNetwork.LocalPlayer == null) return;
        selectedIndex = PlayerNetwork.LocalPlayer.SelectedCharacterIndex.Value;
        var data = allCharacters[Mathf.Clamp(selectedIndex, 0, allCharacters.Length - 1)];
        nameText.text = data.characterName;
        portrait.sprite = data.portrait;

        bool locked = PlayerNetwork.LocalPlayer.IsReady.Value;
        prevButton.interactable = !locked;
        nextButton.interactable = !locked;
    }
}
