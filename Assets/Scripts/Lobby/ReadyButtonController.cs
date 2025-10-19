using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class ReadyButtonController : MonoBehaviour
{
    private Button readyButton;
    private bool isReady = false;
    private PlayerCardUI localCard;

    void Start()
    {
        readyButton = GetComponent<Button>();
        readyButton.onClick.AddListener(ToggleReady);

        Invoke(nameof(FindLocalCard), 0.5f);
    }

    void FindLocalCard()
    {
        foreach (var card in FindObjectsOfType<PlayerCardUI>())
        {
            if (card.OwnerClientId == NetworkManager.Singleton.LocalClientId)
            {
                localCard = card;
                break;
            }
        }
    }

    void ToggleReady()
    {
        if (localCard == null)
        {
            FindLocalCard();
            if (localCard == null) return;
        }

        isReady = !isReady;
        localCard.SetReadyServerRpc(isReady);
    }
}
