using UnityEngine;

public class CharacterStoreTester : MonoBehaviour
{
    void Start()
    {
        if (CharacterSelectionStore.Instance == null)
        {
            Debug.LogError("❌ No CharacterSelectionStore found in the scene!");
            return;
        }

        Debug.Log($"✅ Found CharacterSelectionStore with {CharacterSelectionStore.Instance.characters.Count} characters.");

        for (int i = 0; i < CharacterSelectionStore.Instance.characters.Count; i++)
        {
            var data = CharacterSelectionStore.Instance.characters[i];
            if (data == null)
            {
                Debug.LogWarning($"⚠️ Character slot {i} is empty!");
                continue;
            }

            Debug.Log($"[{i}] {data.characterName} | Prefab: {(data.characterPrefab ? data.characterPrefab.name : "None")} | Icon: {(data.characterIcon ? data.characterIcon.name : "None")}");
        }
    }

    void Update()
    {
        // Optional: Press 1–4 to spawn each character prefab for quick testing.
        if (Input.GetKeyDown(KeyCode.Alpha1)) SpawnCharacter(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SpawnCharacter(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SpawnCharacter(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SpawnCharacter(3);
    }

    void SpawnCharacter(int index)
    {
        var data = CharacterSelectionStore.Instance.GetCharacterByIndex(index);
        if (data == null || data.characterPrefab == null)
        {
            Debug.LogWarning($"❌ Character {index} missing prefab!");
            return;
        }

        var obj = Instantiate(data.characterPrefab, new Vector3(index * 2, 0, 0), Quaternion.identity);
        Debug.Log($"🧍 Spawned {data.characterName} at {obj.transform.position}");
    }
}
