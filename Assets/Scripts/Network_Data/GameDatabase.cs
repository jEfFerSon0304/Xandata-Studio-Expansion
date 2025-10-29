using Unity.Netcode;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GameDatabase : NetworkBehaviour
{
    public static GameDatabase Instance { get; private set; }

    [Header("Characters (0=Eagle,1=Tarsier,2=Carabao,3=Whale)")]
    public CharacterDataSO[] allCharacters;

    public Dictionary<ulong, int> chosenCharacters = new();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetCharacter(ulong clientId, int characterIndex)
    {
        chosenCharacters[clientId] = characterIndex;
        PushSyncToClients(); // 🧩 Auto-sync when new data is set
    }

    public CharacterDataSO GetCharacterData(ulong clientId)
    {
        if (chosenCharacters.TryGetValue(clientId, out int index))
        {
            if (index >= 0 && index < allCharacters.Length)
                return allCharacters[index];
        }
        return null;
    }

    public string GetCharacterName(ulong clientId)
    {
        var data = GetCharacterData(clientId);
        return data != null ? data.characterName : $"Player {clientId}";
    }

    // 🧩 --- NETWORK SYNCING ---
    void PushSyncToClients()
    {
        if (IsServer)
        {
            var ids = chosenCharacters.Keys.ToArray();
            var indices = chosenCharacters.Values.ToArray();
            SyncChosenCharactersClientRpc(ids, indices);
        }
    }

    [ClientRpc]
    void SyncChosenCharactersClientRpc(ulong[] ids, int[] indices)
    {
        chosenCharacters.Clear();
        for (int i = 0; i < ids.Length; i++)
            chosenCharacters[ids[i]] = indices[i];
    }
}
