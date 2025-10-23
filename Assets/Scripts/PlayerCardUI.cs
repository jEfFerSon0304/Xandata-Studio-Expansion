using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerCardUI : MonoBehaviour
{
    public Image portrait;
    public TMP_Text nameText;
    public Button prev, next;
    public GameObject readyOverlay;

    [HideInInspector] public ulong ownerId;

    public void SetData(Sprite pic, string name, bool ready)
    {
        if (portrait) portrait.sprite = pic;
        if (nameText) nameText.text = name;
        if (readyOverlay) readyOverlay.SetActive(ready);
    }

    public void ShowControls(bool show)
    {
        if (prev) prev.gameObject.SetActive(show);
        if (next) next.gameObject.SetActive(show);
    }
}
