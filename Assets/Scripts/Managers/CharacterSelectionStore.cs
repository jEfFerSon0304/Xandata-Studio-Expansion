using System.Collections.Generic;
using UnityEngine;

public class CharacterSelectionStore : MonoBehaviour
{
    public static CharacterSelectionStore Instance;
    public List<CharacterDataSO> characters = new();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public CharacterDataSO GetCharacterByIndex(int index)
    {
        if (index < 0 || index >= characters.Count) return null;
        return characters[index];
    }
}
