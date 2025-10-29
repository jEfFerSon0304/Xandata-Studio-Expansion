using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

public class MainGameTrophyWidget : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text trophyCountText;
    public Button addTrophyButton;
    public Button removeTrophyButton;
    public GameObject confirmationPopupPanel;
    public TMP_Text confirmationMessage;
    public Button yesButton;
    public Button noButton;

    private ulong myClientId;

    void OnEnable()
    {
        GameState.OnTurnOrderChangedEvent += UpdateTrophyCount;
    }

    void OnDisable()
    {
        GameState.OnTurnOrderChangedEvent -= UpdateTrophyCount;
    }

    void Start()
    {
        myClientId = NetworkManager.Singleton.LocalClientId;

        addTrophyButton.onClick.RemoveAllListeners();
        removeTrophyButton.onClick.RemoveAllListeners();

        addTrophyButton.onClick.AddListener(OnAddTrophyClicked);
        removeTrophyButton.onClick.AddListener(OnRemoveTrophyClicked);

        confirmationPopupPanel.SetActive(false);
        UpdateTrophyCount();
    }

    void OnAddTrophyClicked()
    {
        confirmationMessage.text = "Are you sure you want to add a trophy?";
        confirmationPopupPanel.SetActive(true);

        yesButton.onClick.RemoveAllListeners();
        noButton.onClick.RemoveAllListeners();

        yesButton.onClick.AddListener(() =>
        {
            if (GameState.Instance != null)
            {
                GameState.Instance.AddTrophyServerRpc(myClientId);
                Debug.Log($"[TrophyWidget] Player {myClientId} added a trophy!");
            }
            confirmationPopupPanel.SetActive(false);
        });

        noButton.onClick.AddListener(() =>
        {
            confirmationPopupPanel.SetActive(false);
        });
    }

    void OnRemoveTrophyClicked()
    {
        if (GameState.Instance != null)
        {
            GameState.Instance.RemoveTrophyServerRpc(myClientId);
            Debug.Log($"[TrophyWidget] Player {myClientId} removed a trophy!");
        }
    }

    void UpdateTrophyCount()
    {
        if (GameState.Instance == null) return;

        int count = GameState.Instance.GetTrophyCount(myClientId);
        trophyCountText.text = count.ToString();
    }
}
