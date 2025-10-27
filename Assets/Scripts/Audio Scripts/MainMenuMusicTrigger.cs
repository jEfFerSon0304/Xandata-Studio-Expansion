using UnityEngine;

public class MainMenuMusicTrigger : MonoBehaviour
{
    [Header("Main Menu Music")]
    public AudioClip mainMenuMusic;

    void Start()
    {
        // Plays the music only when entering this scene
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayMusic(mainMenuMusic);
        }
    }
}
