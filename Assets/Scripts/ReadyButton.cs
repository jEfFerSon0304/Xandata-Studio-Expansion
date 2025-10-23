using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using System.Linq;

public class ReadyButton : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text buttonText;

    private Button button;

    void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    void OnEnable()
    {
        PlayerNetwork.OnAnyReadyStateChanged += UpdateLabel;
    }

    void OnDisable()
    {
        PlayerNetwork.OnAnyReadyStateChanged -= UpdateLabel;
    }

    void Start()
    {
        if (buttonText == null)
            buttonText = GetComponentInChildren<TMP_Text>();

        Invoke(nameof(UpdateLabel), 0.3f);
    }

    void OnClick()
    {
        if (NetworkManager.Singleton == null) return;

        if (NetworkManager.Singleton.IsHost)
        {
            if (AllPlayersReady())
                LobbyManager.Instance.TryStartGame();
        }
        else
        {
            if (PlayerNetwork.LocalPlayer == null) return;

            bool current = PlayerNetwork.LocalPlayer.IsReady.Value;
            PlayerNetwork.LocalPlayer.SetReadyServerRpc(!current);
        }

        UpdateLabel();
    }

    void UpdateLabel()
    {
        if (buttonText == null || NetworkManager.Singleton == null) return;

        if (NetworkManager.Singleton.IsHost)
        {
            if (AllPlayersReady())
            {
                buttonText.text = "Start Game";
            }
            else
            {
                buttonText.text = "Waiting...";
            }
        }
        else
        {
            if (PlayerNetwork.LocalPlayer == null) return;

            if (PlayerNetwork.LocalPlayer.IsReady.Value)
            {
                buttonText.text = "Locked 🔒";
            }
            else
            {
                buttonText.text = "Ready";
            }
        }

        Debug.Log($"[Host] All ready? {AllPlayersReady()}");
    }

    bool AllPlayersReady()
    {
        var players = FindObjectsOfType<PlayerNetwork>();
        return players.Length > 0 && players.All(p => p.IsReady.Value);
    }
}
