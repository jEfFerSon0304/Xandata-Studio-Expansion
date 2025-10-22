using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

public class ReadyButton : MonoBehaviour
{
    public TMP_Text buttonText;
    private Button button;

    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnButtonClick);
        UpdateLabel();
    }

    void UpdateLabel()
    {
        buttonText.text = NetworkManager.Singleton.IsHost ? "Start" : "Ready";
    }

    void OnButtonClick()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            LobbyManager.Instance.TryStartGame();
        }
        else
        {
            bool newState = !PlayerNetwork.LocalPlayer.IsReady.Value;
            PlayerNetwork.LocalPlayer.SetReadyServerRpc(newState);
        }
    }
}
