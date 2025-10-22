using UnityEngine;
using Unity.Netcode;

public class PlayerNetwork : NetworkBehaviour
{
    public static PlayerNetwork LocalPlayer;

    // Network-synchronized variables
    public NetworkVariable<int> SelectedCharacterIndex =
        new(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<bool> IsReady =
        new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
            LocalPlayer = this;
    }

    [ServerRpc]
    public void SetCharacterIndexServerRpc(int index)
    {
        SelectedCharacterIndex.Value = index;
        Debug.Log($"[Server] Player {OwnerClientId} selected character index {index}");
    }

    [ServerRpc]
    public void SetReadyServerRpc(bool ready)
    {
        IsReady.Value = ready;
    }
}
