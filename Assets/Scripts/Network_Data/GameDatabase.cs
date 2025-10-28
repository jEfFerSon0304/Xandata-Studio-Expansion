using System.Collections.Generic;
using UnityEngine;
using static CharacterDataSO;

public class GameDatabase : MonoBehaviour
{
    public static GameDatabase Instance { get; private set; }

    [Header("Characters (0=Eagle,1=Tarsier,2=Carabao,3=Whale)")]
    public CharacterDataSO[] allCharacters;

    // Key = ClientId, Value = Character index
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

    /// <summary>
    /// Called by Lobby when player locks a character.
    /// </summary>
    public void SetCharacter(ulong clientId, int characterIndex)
    {
        chosenCharacters[clientId] = characterIndex;
    }

    /// <summary>
    /// Get the CharacterDataSO for a specific player.
    /// </summary>
    public CharacterDataSO GetCharacterData(ulong clientId)
    {
        if (chosenCharacters.TryGetValue(clientId, out int index))
        {
            if (index >= 0 && index < allCharacters.Length)
                return allCharacters[index];
        }

        // Prevent warning spam by ignoring uninitialized players
        // Debug.LogWarning($"[GameDatabase] No character assigned for player {clientId}");
        return null;
    }


    /// <summary>
    /// Helper: get character name
    /// </summary>
    public string GetCharacterName(ulong clientId)
    {
        var data = GetCharacterData(clientId);
        return data != null ? data.characterName : "Unknown";
    }

    /// <summary>
    /// Helper: get skill list
    /// </summary>
    public SkillData[] GetSkills(ulong clientId)
    {
        var data = GetCharacterData(clientId);
        return data != null ? data.skills : null;
    }
}
