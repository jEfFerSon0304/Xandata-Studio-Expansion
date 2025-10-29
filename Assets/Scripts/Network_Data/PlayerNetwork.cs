using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System.Linq;

public class PlayerNetwork : NetworkBehaviour
{
    public static PlayerNetwork LocalPlayer;

    [Header("Local UI Prefab")]
    [Tooltip("Assign the MainGameManager prefab here (non-networked)")]
    public GameObject mainGameUIPrefab;

    public NetworkVariable<int> SlotIndex =
        new(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public NetworkVariable<int> SelectedCharacterIndex =
        new(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public NetworkVariable<bool> IsReady =
        new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public NetworkVariable<bool> isStunned =
        new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private GameObject localUIInstance; // store reference to spawned UI

    public GameObject gameStatePrefab; // 👈 assign in Inspector (Server prefab)


    void Awake() => DontDestroyOnLoad(gameObject);

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            LocalPlayer = this;

            // Spawn MainGame UI (already existing code)
            if (mainGameUIPrefab != null)
            {
                Debug.Log($"[UI] Spawning local MainGameManager UI for player {OwnerClientId}");
                localUIInstance = Instantiate(mainGameUIPrefab);
                localUIInstance.name = $"MainGameUI_{OwnerClientId}";
            }
        }

        // ✅ SERVER ONLY: Spawn the GameState if it doesn't exist yet
        if (IsServer && GameState.Instance == null)
        {
            var gs = Instantiate(gameStatePrefab);
            gs.GetComponent<NetworkObject>().Spawn(true);
            Debug.Log("[Server] Spawned GameState network object.");
        }

        if (IsServer)
            StartCoroutine(AssignSlotAndCharacter());
    }

    private IEnumerator AssignSlotAndCharacter()
    {
        while (GameDatabase.Instance == null || GameDatabase.Instance.allCharacters.Length == 0)
            yield return null;

        var db = GameDatabase.Instance;
        var players = FindObjectsByType<PlayerNetwork>(FindObjectsSortMode.None)
                       .OrderBy(p => p.OwnerClientId).ToList();

        SlotIndex.Value = players.IndexOf(this);

        if (SelectedCharacterIndex.Value < 0)
            SelectedCharacterIndex.Value = SlotIndex.Value % db.allCharacters.Length;

        // ✅ Register character assignment in the shared GameDatabase
        // After registering character
        GameDatabase.Instance.SetCharacter(OwnerClientId, SelectedCharacterIndex.Value);
        UpdateCharacterClientRpc(OwnerClientId, SelectedCharacterIndex.Value);

        SyncCharacterToClientsClientRpc(OwnerClientId, SelectedCharacterIndex.Value);


        IsReady.Value = false;
        Debug.Log($"[ASSIGN] Player {OwnerClientId} | Slot {SlotIndex.Value} | Character {SelectedCharacterIndex.Value}");
        PlayerNetwork.NotifyUpdate();
    }

    [ClientRpc]
    void UpdateCharacterClientRpc(ulong clientId, int charIndex)
    {
        if (GameDatabase.Instance != null)
            GameDatabase.Instance.SetCharacter(clientId, charIndex);
    }


    public static void NotifyUpdate() =>
        OnAnyStateChanged?.Invoke();

    public static event System.Action OnAnyStateChanged;

    [ServerRpc(RequireOwnership = false)]
    public void SetCharacterIndexServerRpc(int index)
    {
        if (!IsReady.Value)
            SelectedCharacterIndex.Value = index;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetReadyServerRpc(bool ready)
    {
        if (ready && SelectedCharacterIndex.Value < 0)
            return;

        IsReady.Value = ready;
        NotifyUpdate();
    }

    private void OnDestroy()
    {
        // 🧹 Cleanup local UI if player disconnects
        if (IsOwner && localUIInstance != null)
        {
            Destroy(localUIInstance);
            localUIInstance = null;
        }
    }

    [ClientRpc]
    void SyncCharacterToClientsClientRpc(ulong clientId, int characterIndex)
    {
        if (GameDatabase.Instance != null)
        {
            GameDatabase.Instance.SetCharacter(clientId, characterIndex);
            Debug.Log($"[Sync] Client received character {characterIndex} for player {clientId}");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestCharacterSyncServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong senderId = rpcParams.Receive.SenderClientId;
        Debug.Log($"[PlayerNetwork] Client {senderId} requested character sync.");

        foreach (var pn in FindObjectsByType<PlayerNetwork>(FindObjectsSortMode.None))
        {
            SyncCharacterToClientsClientRpc(pn.OwnerClientId, pn.SelectedCharacterIndex.Value,
                new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new[] { senderId } } });
        }
    }

    [ClientRpc]
    void SyncCharacterToClientsClientRpc(ulong clientId, int characterIndex, ClientRpcParams rpcParams = default)
    {
        if (GameDatabase.Instance != null)
        {
            GameDatabase.Instance.SetCharacter(clientId, characterIndex);
            Debug.Log($"[Sync] Client received character {characterIndex} for player {clientId}");
        }
    }

}
