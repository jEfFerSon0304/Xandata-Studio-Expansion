using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System.Linq;

public class PlayerNetwork : NetworkBehaviour
{
    public static PlayerNetwork LocalPlayer;

    public NetworkVariable<int> SlotIndex =
        new(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public NetworkVariable<int> SelectedCharacterIndex =
        new(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public NetworkVariable<bool> IsReady =
        new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    void Awake() => DontDestroyOnLoad(gameObject);

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
            LocalPlayer = this;

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

        IsReady.Value = false; // ✅ Always unlocked on lobby load

        Debug.Log($"[ASSIGN] Player {OwnerClientId} | Slot {SlotIndex.Value} | Character {SelectedCharacterIndex.Value}");
        PlayerNetwork.NotifyUpdate();
    }

    public static void NotifyUpdate() =>
        OnAnyStateChanged?.Invoke();

    public static event System.Action OnAnyStateChanged;

    [ServerRpc(RequireOwnership = false)]
    public void SetCharacterIndexServerRpc(int index)
    {
        if (!IsReady.Value) // ✅ Block change when locked
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
}
