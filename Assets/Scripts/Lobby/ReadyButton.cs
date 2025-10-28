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
        InvokeRepeating(nameof(UpdateLabel), 0.2f, 0.2f);
    }

    void OnClick()
    {
        if (PlayerNetwork.LocalPlayer == null) return;

        if (NetworkManager.Singleton.IsHost && AllReady())
        {
            LobbyManager.Instance.TryStartGame();
            return;
        }

        bool newState = !PlayerNetwork.LocalPlayer.IsReady.Value;
        PlayerNetwork.LocalPlayer.SetReadyServerRpc(newState);
    }

    void UpdateLabel()
    {
        if (!NetworkManager.Singleton.IsConnectedClient || PlayerNetwork.LocalPlayer == null)
        {
            label.text = "Loading...";
            button.interactable = false;
            return;
        }

        bool ready = PlayerNetwork.LocalPlayer.IsReady.Value;
        bool allReady = AllReady();
        bool isHost = NetworkManager.Singleton.IsHost;

        if (isHost && allReady)
        {
            label.text = "Start Game";
            button.interactable = true;
        }
        else
        {
            label.text = ready ? "Locked" : "Ready";
            button.interactable = true;
        }
    }

    bool AllReady()
    {
#if UNITY_2023_1_OR_NEWER
        var players = FindObjectsByType<PlayerNetwork>(FindObjectsSortMode.None);
#else
        var players = FindObjectsOfType<PlayerNetwork>();
#endif

        if (players.Length < 2) return false; // require >1 players

        return players.All(p => p.IsReady.Value);
    }
}
