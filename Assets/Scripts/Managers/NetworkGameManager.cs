using Unity.Netcode;
using UnityEngine;

public class NetworkGameManager : NetworkBehaviour
{
    public static NetworkGameManager Instance;
    public Transform[] spawnPoints;

    private void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
            SpawnAllPlayers();
    }

    private void SpawnAllPlayers()
    {
        int index = 0;
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            var playerData = client.PlayerObject.GetComponent<PlayerData>();
            int selectedCharacter = playerData ? playerData.CharacterIndex.Value : 0;
            SpawnCharacter(client.ClientId, selectedCharacter, index);
            index++;
        }
    }

    private void SpawnCharacter(ulong clientId, int charIndex, int spawnIndex)
    {
        var data = CharacterSelectionStore.Instance.GetCharacterByIndex(charIndex);
        if (data == null) return;

        Transform spawn = spawnPoints[spawnIndex % spawnPoints.Length];
        GameObject obj = Instantiate(data.characterPrefab, spawn.position, spawn.rotation);
        obj.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);

        var baseComp = obj.GetComponent<CharacterBase>();
        if (baseComp) baseComp.LoadData(data);
    }
}
