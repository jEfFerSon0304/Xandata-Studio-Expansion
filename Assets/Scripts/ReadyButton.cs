using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

public class ReadyButton : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text buttonText;

    private Button button;

    void Awake()
    {
        button = GetComponent<Button>();
        if (button == null)
        {
            Debug.LogError("❌ ReadyButton: No Button component found on this object!");
            return;
        }

        button.onClick.AddListener(OnClick);
    }

    void Start()
    {
        // Prevent null crash if buttonText is missing or NetworkManager not yet initialized
        if (buttonText == null)
        {
            Debug.LogWarning("⚠ ReadyButton: ButtonText not assigned in Inspector!");
            return;
        }

        if (NetworkManager.Singleton == null)
        {
            Debug.LogWarning("⚠ NetworkManager not found when ReadyButton started.");
            return;
        }

        UpdateLabel();
    }

    void UpdateLabel()
    {
        if (buttonText == null || NetworkManager.Singleton == null) return;

        if (NetworkManager.Singleton.IsHost)
            buttonText.text = "Start Game";
        else
            buttonText.text = "Ready";
    }

    void OnClick()
    {
        if (NetworkManager.Singleton == null) return;

        if (NetworkManager.Singleton.IsHost)
        {
            LobbyManager.Instance.TryStartGame();
        }
        else
        {
            if (PlayerNetwork.LocalPlayer != null)
            {
                bool current = PlayerNetwork.LocalPlayer.IsReady.Value;
                PlayerNetwork.LocalPlayer.SetReadyServerRpc(!current);
            }
        }
    }
}
