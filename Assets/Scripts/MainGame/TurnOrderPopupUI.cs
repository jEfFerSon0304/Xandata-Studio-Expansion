using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.Netcode;

public class TurnOrderPopupUI : MonoBehaviour
{
    [Header("References")]
    public GameObject panelRoot;
    public TMP_Text titleText;
    public Transform listParent;
    public GameObject listItemPrefab;
    public Button closeButton;

    private void Awake()
    {
        panelRoot.SetActive(false);
        closeButton.onClick.AddListener(() => panelRoot.SetActive(false));
    }

    private void OnEnable()
    {
        GameState.OnTurnOrderChangedEvent += Refresh;
    }

    private void OnDisable()
    {
        GameState.OnTurnOrderChangedEvent -= Refresh;
    }

    public void TogglePanel()
    {
        bool newState = !panelRoot.activeSelf;
        panelRoot.SetActive(newState);

        if (newState)
        {
            // 🟢 Ensure latest turn order and character sync for clients
            if (!NetworkManager.Singleton.IsServer)
            {
                if (GameState.Instance != null)
                    GameState.Instance.RequestInitialTurnOrderServerRpc();

                var myPlayer = PlayerNetwork.LocalPlayer;
                if (myPlayer != null)
                    myPlayer.RequestCharacterSyncServerRpc();
            }

            Refresh();
        }
    }

    private void Refresh()
    {
        if (GameState.Instance == null) return;

        foreach (Transform child in listParent)
            Destroy(child.gameObject);

        IEnumerable<ulong> order = GameState.Instance.GetCurrentTurnOrder();
        int position = 1;

        foreach (var clientId in order)
        {
            var item = Instantiate(listItemPrefab, listParent);
            var text = item.GetComponentInChildren<TMP_Text>();

            string charName = GameDatabase.Instance != null
                ? GameDatabase.Instance.GetCharacterName(clientId)
                : $"Player {clientId}";

            if (string.IsNullOrEmpty(charName))
                charName = $"Player {clientId}";

            string colorHex = "FFFFFF";
            var data = GameDatabase.Instance?.GetCharacterData(clientId);
            if (data != null)
                colorHex = ColorUtility.ToHtmlStringRGB(data.themeColor);

            int trophyCount = GameState.Instance.GetTrophyCount(clientId);

            text.text = $"<b>{position}.</b> <color=#{colorHex}>{charName}</color> ⭐x{trophyCount}";
            position++;
        }
    }
}
