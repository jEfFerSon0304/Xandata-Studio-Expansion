using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerCardUI : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text playerLabel;      // ← add this
    public Image portrait;
    public TMP_Text characterNameText;
    public Image readyIndicator;
    public Button prevButton;
    public Button nextButton;

    [HideInInspector] public ulong ownerClientId;
    [HideInInspector] public bool isLocal;

    public void SetPlayerLabel(string label)
    {
        if (playerLabel != null)
            playerLabel.text = label;
    }

    public void SetCharacter(Sprite sprite, string name)
    {
        if (portrait != null) portrait.sprite = sprite;
        if (characterNameText != null) characterNameText.text = name;
    }

    public void SetReadyVisual(bool ready)
    {
        if (readyIndicator != null)
            readyIndicator.enabled = ready;
    }

    public void ShowNavigationButtons(bool show)
    {
        if (prevButton != null) prevButton.gameObject.SetActive(show);
        if (nextButton != null) nextButton.gameObject.SetActive(show);
    }
}
