using UnityEngine;

public class GameDatabase : MonoBehaviour
{
    public static GameDatabase Instance;

    public CharacterDataSO[] characters;
    //public SkillDataSO[] skills; // unused now

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
}
