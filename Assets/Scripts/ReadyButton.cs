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
        button.onClick.AddListener(Click);
    }

    void Start()
    {
        InvokeRepeating(nameof(UpdateLabel), 0.2f, 0.2f);
    }

    void Click()
    {
        if (PlayerNetwork.LocalPlayer == null) return;

        if (PlayerNetwork.LocalPlayer.State == null)
        {
            Debug.Log("State not ready");
            return;
        }

        bool ready = PlayerNetwork.LocalPlayer.State.IsReady.Value;

        if (NetworkManager.Singleton.IsHost && AllReady())
        {
            LobbyManager.Instance.TryStartGame();
            return;
        }

        PlayerNetwork.LocalPlayer.SetReadyServerRpc(!ready);
    }

    void UpdateLabel()
    {
        if (PlayerNetwork.LocalPlayer == null)
        {
            label.text = "Loading...";
            button.interactable = false;
            return;
        }

        if (PlayerNetwork.LocalPlayer.State == null)
        {
            label.text = "Syncing...";
            return; // ✅ remove button disable so UI continues checking
        }


        bool ready = PlayerNetwork.LocalPlayer.State.IsReady.Value;
        bool allReady = AllReady();
        bool isHost = NetworkManager.Singleton.IsHost;

        if (isHost && allReady)
        {
            label.text = "Start Game";
        }
        else
        {
            label.text = ready ? "Locked" : "Ready";
        }

        button.interactable = true;
    }

    bool AllReady()
    {
        var list = FindObjectsByType<PlayerNetwork>(FindObjectsSortMode.None);
        if (list.Length < 2) return false;
        return list.All(p => p.State != null && p.State.IsReady.Value);
    }
}
