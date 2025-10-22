using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    [Header("Spawn Points")]
    public Transform[] spawnPoints;
    public GameObject[] characterPrefabs; // Character_0, 1, 2, 3

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        var players = FindObjectsOfType<PlayerNetwork>();
        for (int i = 0; i < players.Length; i++)
        {
            int charIndex = Mathf.Clamp(players[i].SelectedCharacterIndex.Value, 0, 3);
            var spawn = spawnPoints[i % spawnPoints.Length];
            GameObject obj = Instantiate(characterPrefabs[charIndex], spawn.position, spawn.rotation);
            obj.GetComponent<NetworkObject>().SpawnAsPlayerObject(players[i].OwnerClientId);
        }
    }
}
