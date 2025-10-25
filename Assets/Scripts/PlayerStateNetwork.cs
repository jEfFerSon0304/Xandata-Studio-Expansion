using UnityEngine;
using Unity.Netcode;

public class PlayerStateNetwork : NetworkBehaviour
{
    public NetworkVariable<bool> IsReady =
        new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
}
