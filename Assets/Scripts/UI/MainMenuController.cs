using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class MainMenuController : MonoBehaviour
{
    public void HostGame()
    {
        EnsureNetworkManager();

        if (!NetworkManager.Singleton.IsServer && !NetworkManager.Singleton.IsClient)
        {
            NetworkManager.Singleton.StartHost();
        }

        SceneManager.LoadScene("LobbyScene");
    }

    public void JoinGame()
    {
        EnsureNetworkManager();

        if (!NetworkManager.Singleton.IsServer && !NetworkManager.Singleton.IsClient)
        {
            NetworkManager.Singleton.StartClient();
        }

        SceneManager.LoadScene("LobbyScene");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private void EnsureNetworkManager()
    {
        if (NetworkManager.Singleton == null)
        {
            var prefab = Resources.Load<GameObject>("Prefabs/Network/NetworkManager");
            Instantiate(prefab);
        }
    }
}
