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
        [Header("Basic Info")]
        public string skillName;
        [TextArea] public string description;
        public int energyCost;
        public bool isUltimate;

        [Header("Visual")]
        public Sprite skillIcon;

        [Header("Target Settings")]
        public bool requiresTarget; // ✅ NEW FIELD — Does this skill need a target?

        [Header("Reveal Animation Settings")]
        public SkillAnimationSO animationData;
    }
}
