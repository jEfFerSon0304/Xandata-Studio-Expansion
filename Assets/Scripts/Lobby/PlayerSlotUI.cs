using UnityEngine;

public class PlayerSlotUI : MonoBehaviour
{
    public GameObject waitingText;
    public Transform cardHolder;

    public void ShowWaiting(bool state)
    {
        waitingText.SetActive(state);
    }

    public void SetCard(GameObject card)
    {
        ShowWaiting(false);
        card.transform.SetParent(cardHolder, false);
    }

    public void Clear()
    {
        foreach (Transform child in cardHolder)
            Destroy(child.gameObject);

        ShowWaiting(true);
    }
}
