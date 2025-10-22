using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class LobbyManager : NetworkBehaviour
{
    public static LobbyManager Instance;

    // Optional: lobby code if you want to display one later
    [SerializeField] private string lobbyCode;

    // Cache of connected players
    private readonly List<PlayerNetwork> connectedPlayers = new();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"Client connected: {clientId}");
        RefreshConnectedPlayers();
    }

    private void OnClientDisconnected(ulong clientId)
    {
        Debug.Log($"Client disconnected: {clientId}");
        RefreshConnectedPlayers();
    }

    private void RefreshConnectedPlayers()
    {
        connectedPlayers.Clear();
        foreach (var player in FindObjectsOfType<PlayerNetwork>())
        {
            connectedPlayers.Add(player);
        }
    }

    // Called by the host's ReadyButton
    public void TryStartGame()
    {
        if (!IsServer) return;

        if (!AllPlayersReady())
        {
            Debug.LogWarning("Not all players are ready yet!");
            return;
        }

        Debug.Log("All players ready — starting game...");
        NetworkManager.Singleton.SceneManager.LoadScene("Game", LoadSceneMode.Single);
    }

    private bool AllPlayersReady()
    {
        foreach (var player in FindObjectsOfType<PlayerNetwork>())
        {
            if (!player.IsReady.Value)
                return false;
        }
        return true;
    }

    // Optional: Helper to get player data by clientId
    public PlayerNetwork GetPlayer(ulong clientId)
    {
        foreach (var player in connectedPlayers)
        {
            if (player.OwnerClientId == clientId)
                return player;
        }
        return null;
    }

    // Optional: For UI display later
    public List<PlayerNetwork> GetAllPlayers()
    {
        return new List<PlayerNetwork>(connectedPlayers);
    }
}
