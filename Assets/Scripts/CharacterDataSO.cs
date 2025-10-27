using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "Game/Character Data")]
public class CharacterDataSO : ScriptableObject
{
    [Header("Character Info")]
    public string characterName;
    public Sprite portrait;
    public SkillData[] skills; // 3 skills
    public Color themeColor = Color.white;


    [Header("Description")]
    [TextArea]
    public string description;

    [Header("Prefab Reference (optional for later gameplay)")]
    public GameObject characterPrefab;

    [System.Serializable]
    public class SkillData
    {
        public string skillName;
        public string description;
        public int energyCost;
        public bool isUltimate;

        [Header("Visual")]
        public Sprite skillIcon;
    }

}
