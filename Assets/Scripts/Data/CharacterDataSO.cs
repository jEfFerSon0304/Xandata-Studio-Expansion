using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "Game/Character Data")]
public class CharacterDataSO : ScriptableObject
{
    public string characterName;
    public Sprite characterIcon;
    public GameObject characterPrefab;
    public RuntimeAnimatorController animatorController;

    [Header("Stats")]
    public float maxEnergy = 5;

}
