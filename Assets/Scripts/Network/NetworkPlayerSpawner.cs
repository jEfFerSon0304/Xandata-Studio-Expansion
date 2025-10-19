using Unity.Netcode;
using UnityEngine;

public class NetworkPlayerSpawner : NetworkBehaviour
{
    [ServerRpc(RequireOwnership = false)]
    public void SaveSelectedCharacterServerRpc(int selectedIndex, ServerRpcParams rpcParams = default)
    {
        ulong senderId = rpcParams.Receive.SenderClientId;
        PlayerPrefs.SetInt($"Player_{senderId}_Character", selectedIndex);
        Debug.Log($"Saved Player {senderId}'s selected character index: {selectedIndex}");
    }
}
