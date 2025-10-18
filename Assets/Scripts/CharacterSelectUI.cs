using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectUI : MonoBehaviour
{
    public Image characterImagePlaceholder;
    public Text characterNameText;
    public Button readyButton;
    public Sprite[] characterSprites;
    public string[] characterNames;

    private int currentIndex = 0;
    private bool isReady = false;

    void Start()
    {
        UpdateCharacterUI();
    }

    public void SelectNextCharacter()
    {
        int startIndex = currentIndex;
        do
        {
            currentIndex = (currentIndex + 1) % characterSprites.Length;
        } while (CharacterSelectionStore.Instance.IsCharacterLocked(currentIndex) && currentIndex != startIndex);

        UpdateCharacterUI();
        AutoUnready();
        CharacterSelectionStore.Instance.AssignPlayerToSlot(NetworkManager.Singleton.LocalClientId);
    }

    public void SelectPreviousCharacter()
    {
        int startIndex = currentIndex;
        do
        {
            currentIndex = (currentIndex - 1 + characterSprites.Length) % characterSprites.Length;
        } while (CharacterSelectionStore.Instance.IsCharacterLocked(currentIndex) && currentIndex != startIndex);

        UpdateCharacterUI();
        AutoUnready();
        CharacterSelectionStore.Instance.AssignPlayerToSlot(NetworkManager.Singleton.LocalClientId);
    }

    private void UpdateCharacterUI()
    {
        characterImagePlaceholder.sprite = characterSprites[currentIndex];
        characterNameText.text = characterNames[currentIndex];
    }

    private void AutoUnready()
    {
        if (isReady)
        {
            isReady = false;
            readyButton.GetComponentInChildren<Text>().text = "Ready";
            CharacterSelectionStore.Instance.SetReady(NetworkManager.Singleton.LocalClientId, false);
        }
    }

    public void ToggleReady()
    {
        isReady = !isReady;
        readyButton.GetComponentInChildren<Text>().text = isReady ? "Unready" : "Ready";
        CharacterSelectionStore.Instance.SetReady(NetworkManager.Singleton.LocalClientId, isReady);
    }
}
