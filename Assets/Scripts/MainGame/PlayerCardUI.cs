using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerCardUI : MonoBehaviour
{
    public Image portrait;
    public TMP_Text nameText;
    public Button prev;
    public Button next;
    public GameObject readyOverlay;

    [HideInInspector] public ulong ownerId;

    public void SetData(Sprite pic, string name, bool ready)
    {
        portrait.sprite = pic;
        nameText.text = name;
        readyOverlay.SetActive(ready);
    }

    public void ShowControls(bool show)
    {
        prev.gameObject.SetActive(show);
        next.gameObject.SetActive(show);
    }
}
