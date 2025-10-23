using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using System.Linq;

public class ReadyButton : MonoBehaviour
{
    public TMP_Text label;
    Button button;

    void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    void Start()
    {
        if (label == null) label = GetComponentInChildren<TMP_Text>();
        PlayerNetwork.OnAnyStateChanged += UpdateLabel;
        InvokeRepeating(nameof(UpdateLabel), 0.3f, 0.3f);
    }

    void OnDestroy() => PlayerNetwork.OnAnyStateChanged -= UpdateLabel;

    void OnClick()
    {
        if (NetworkManager.Singleton == null) return;

        if (NetworkManager.Singleton.IsHost && AllPlayersReady())
        {
            Debug.Log("[Host] Everyone ready — starting game...");
            LobbyManager.Instance.TryStartGame();
            return;
        }

        if (PlayerNetwork.LocalPlayer != null)
        {
            bool ready = PlayerNetwork.LocalPlayer.IsReady.Value;
            PlayerNetwork.LocalPlayer.SetReadyServerRpc(!ready);
        }
    }

    void UpdateLabel()
    {
        if (label == null || NetworkManager.Singleton == null) return;

        bool allReady = AllPlayersReady();

        if (NetworkManager.Singleton.IsHost)
        {
            // Host acts like everyone else until all ready
            if (allReady)
            {
                label.text = "Start Game";
                label.color = Color.green;
            }
            else
            {
                label.text = PlayerNetwork.LocalPlayer?.IsReady.Value == true ? "Locked" : "Ready";
                label.color = PlayerNetwork.LocalPlayer?.IsReady.Value == true ? Color.gray : Color.cyan;
            }
        }
        else
        {
            if (PlayerNetwork.LocalPlayer == null) return;
            label.text = PlayerNetwork.LocalPlayer.IsReady.Value ? "Locked" : "Ready";
            label.color = PlayerNetwork.LocalPlayer.IsReady.Value ? Color.gray : Color.cyan;
        }
    }

    bool AllPlayersReady()
    {
#if UNITY_2023_1_OR_NEWER
        var players = FindObjectsByType<PlayerNetwork>(FindObjectsSortMode.None);
#else
        var players = FindObjectsOfType<PlayerNetwork>();
#endif
        if (players.Length == 0) return false;
        return players.All(p => p.IsReady.Value);
    }
}
