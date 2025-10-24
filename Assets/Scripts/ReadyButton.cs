using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using System.Linq;

public class ReadyButton : MonoBehaviour
{
    public TMP_Text label;
    private Button button;

    void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    void Start()
    {
        if (label == null)
            label = GetComponentInChildren<TMP_Text>();

        PlayerNetwork.OnAnyStateChanged += UpdateLabel;
        InvokeRepeating(nameof(UpdateLabel), 0.3f, 0.3f);
    }

    void OnDestroy()
    {
        PlayerNetwork.OnAnyStateChanged -= UpdateLabel;
    }

    void OnClick()
    {
        if (NetworkManager.Singleton == null)
            return;

        if (NetworkManager.Singleton.IsHost)
        {
            if (AllPlayersReady())
            {
                // Host can only start when everyone is ready
                Debug.Log("[Host] All players ready. Starting game...");
                LobbyManager.Instance.TryStartGame();
            }
            else
            {
                // Host toggles own ready state like others
                if (PlayerNetwork.LocalPlayer != null)
                {
                    bool ready = PlayerNetwork.LocalPlayer.IsReady.Value;
                    PlayerNetwork.LocalPlayer.SetReadyServerRpc(!ready);
                }
            }
        }
        else
        {
            // Clients just toggle ready state
            if (PlayerNetwork.LocalPlayer != null)
            {
                bool ready = PlayerNetwork.LocalPlayer.IsReady.Value;
                PlayerNetwork.LocalPlayer.SetReadyServerRpc(!ready);
            }
        }
    }

    void UpdateLabel()
    {
        if (label == null || NetworkManager.Singleton == null)
            return;

        bool allReady = AllPlayersReady();
        bool isHost = NetworkManager.Singleton.IsHost;
        bool isLocalReady = PlayerNetwork.LocalPlayer != null && PlayerNetwork.LocalPlayer.IsReady.Value;

        if (isHost)
        {
            if (allReady)
            {
                label.text = "Start Game";
            }
            else
            {
                label.text = isLocalReady ? "Locked" : "Ready";
            }
        }
        else
        {
            label.text = isLocalReady ? "Locked" : "Ready";
        }

        // Disable button if game not connected or host waiting
        button.interactable = NetworkManager.Singleton.IsConnectedClient;
    }

    bool AllPlayersReady()
    {
#if UNITY_2023_1_OR_NEWER
        var players = FindObjectsByType<PlayerNetwork>(FindObjectsSortMode.None);
#else
        var players = FindObjectsOfType<PlayerNetwork>();
#endif
        if (players.Length == 0)
            return false;

        return players.All(p => p.IsReady.Value);
    }
}
