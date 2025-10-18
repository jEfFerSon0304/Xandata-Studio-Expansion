using UnityEngine;
using Unity.Netcode;

public class LobbyNetworkStarter : MonoBehaviour
{
    public static bool startAsHost = false;

    void Start()
    {
        if (startAsHost)
        {
            NetworkManager.Singleton.StartHost();
        }
        else
        {
            NetworkManager.Singleton.StartClient();
        }
    }
}
