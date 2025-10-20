using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class MenuController : MonoBehaviour
{
    public void HostGame()
    {
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        if (transport != null)
        {
            transport.ConnectionData.Port = 7777; // fixed
            transport.ConnectionData.Address = "127.0.0.1"; // local
            Debug.Log("📡 Hosting on port 7777");
        }

        if (NetworkManager.Singleton.IsListening)
        {
            Debug.LogWarning("⚠️ A host is already running! Cannot start another host.");
            return;
        }

        if (NetworkManager.Singleton.StartHost())
        {
            Debug.Log("👑 Host started successfully!");
            UnityEngine.SceneManagement.SceneManager.LoadScene("LobbyScene");
        }
        else
        {
            Debug.LogError("❌ Failed to start host!");
        }
    }

    public void JoinGame()
    {
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        if (transport != null)
        {
            transport.ConnectionData.Port = 7777; // must match host
            transport.ConnectionData.Address = "127.0.0.1"; // host machine
            Debug.Log("🙋 Connecting to host on port 7777");
        }

        if (NetworkManager.Singleton.StartClient())
        {
            Debug.Log("🙋 Joining lobby...");
            UnityEngine.SceneManagement.SceneManager.LoadScene("LobbyScene");
        }
        else
        {
            Debug.LogError("❌ Failed to start client!");
        }
    }
}
