using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System.Linq;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void TryStartGame()
    {
        if (!NetworkManager.Singleton.IsServer) return;

#if UNITY_2023_1_OR_NEWER
        var players = FindObjectsByType<PlayerNetwork>(FindObjectsSortMode.None);
#else
        var players = FindObjectsOfType<PlayerNetwork>();
#endif
        if (players.Length == 0) return;
        if (!players.All(p => p.IsReady.Value))
        {
            Debug.Log("[LobbyManager] Not all players are ready yet.");
            return;
        }

        Debug.Log("[LobbyManager] All players ready. Loading MainGame scene...");
        NetworkManager.Singleton.SceneManager.LoadScene("DiceRoll", LoadSceneMode.Single);
    }
}
