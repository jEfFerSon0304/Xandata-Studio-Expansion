using UnityEngine;
using UnityEngine.UI;

public class ReadyButtonController : MonoBehaviour
{
    private Button button;
    private bool isReady = false;

    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnReadyClicked);
    }

    private void OnReadyClicked()
    {
        if (PlayerData.LocalPlayer == null)
        {
            Debug.LogWarning("❌ LocalPlayer not found!");
            return;
        }

        isReady = !isReady;

        // Send to server
        PlayerData.LocalPlayer.SetReadyStateServerRpc(isReady);

        // Change button text locally
        var txt = button.GetComponentInChildren<TMPro.TMP_Text>();
        if (txt != null)
            txt.text = isReady ? "Unready" : "Ready";
    }
}
