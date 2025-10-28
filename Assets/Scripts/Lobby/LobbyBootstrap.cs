using UnityEngine;
using Unity.Netcode;

public class LobbyBootstrap : MonoBehaviour
{
    void Start()
    {
        if (!NetworkManager.Singleton || !NetworkManager.Singleton.IsServer)
            return;

#if UNITY_2023_1_OR_NEWER
        var players = FindObjectsByType<PlayerNetwork>(FindObjectsSortMode.None);
#else
        var players = FindObjectsOfType<PlayerNetwork>();
#endif

        foreach (var p in players)
        {
            p.IsReady.Value = false; // ✅ allow selection again
            if (p.SelectedCharacterIndex.Value < 0)
                p.SelectedCharacterIndex.Value = 0; // ✅ prevent invalid
        }

        PlayerNetwork.NotifyUpdate();
    }
}
