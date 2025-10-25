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

        var players = FindObjectsByType<PlayerNetwork>(FindObjectsSortMode.None);
        if (players.Length < 2)
        {
            Debug.Log("Not enough players to start!");
            return;
        }

        if (!players.All(p => p.State != null && p.State.IsReady.Value))
        {
            Debug.Log("Players not ready yet!");
            return;
        }

        NetworkManager.Singleton.SceneManager.LoadScene("DiceRoll",
            LoadSceneMode.Single);
    }
}
