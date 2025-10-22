using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerCardUI : MonoBehaviour
{
    [Header("UI References")]
    public Image portrait;
    public TMP_Text characterNameText;
    public Image readyIndicator;
    public Button prevButton;
    public Button nextButton;

    [HideInInspector] public ulong ownerClientId;
    [HideInInspector] public bool isLocal;

    public void SetCharacter(Sprite sprite, string name)
    {
        portrait.sprite = sprite;
        characterNameText.text = name;
    }

    public void SetReadyVisual(bool ready)
    {
        if (readyIndicator != null)
            readyIndicator.enabled = ready; // Turn on/off image
    }

    public void EnableButtons(bool enable)
    {
        if (prevButton != null) prevButton.interactable = enable;
        if (nextButton != null) nextButton.interactable = enable;
    }
}
