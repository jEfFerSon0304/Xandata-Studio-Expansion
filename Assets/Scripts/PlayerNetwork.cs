using UnityEngine;
using Unity.Netcode;
using System.Linq;
using System.Collections;

public class PlayerNetwork : NetworkBehaviour
{
    public static PlayerNetwork LocalPlayer;
    public PlayerStateNetwork State;

    public NetworkVariable<int> SelectedCharacterIndex =
        new(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public NetworkVariable<int> SlotIndex =
        new(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public override void OnNetworkSpawn()
    {
        State = GetComponent<PlayerStateNetwork>(); // ✅ Move BEFORE LocalPlayer assignment

        if (IsOwner)
            LocalPlayer = this;

        State = GetComponent<PlayerStateNetwork>();

        if (IsServer)
            StartCoroutine(WaitForDBAndAssign());
    }

    private IEnumerator WaitForDBAndAssign()
    {
        while (GameDatabase.Instance == null || GameDatabase.Instance.characters.Length == 0)
            yield return null;

        var db = GameDatabase.Instance;
        var players = FindObjectsByType<PlayerNetwork>(FindObjectsSortMode.None)
            .OrderBy(p => p.OwnerClientId)
            .ToList();

        SlotIndex.Value = players.IndexOf(this);

        if (SelectedCharacterIndex.Value < 0)
            SelectedCharacterIndex.Value = SlotIndex.Value % db.characters.Length;

        Debug.Log($"Assigned: Client {OwnerClientId}, Slot {SlotIndex.Value}, Character {SelectedCharacterIndex.Value}");
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetCharacterIndexServerRpc(int index)
    {
        SelectedCharacterIndex.Value = index;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetReadyServerRpc(bool ready)
    {
        if (State != null)
            State.IsReady.Value = ready;
    }
}
