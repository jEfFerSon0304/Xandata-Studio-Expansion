using UnityEngine;
using UnityEngine.Events;

public class PlaySFXTrigger : MonoBehaviour
{
    [Header("Sound Settings")]
    public AudioClip sfxClip;             // Assign your sound here in Inspector
    public bool playOnStart = false;      // Optional: play automatically when scene starts

    [Header("Optional Events")]
    public UnityEvent onPlaySFX;          // Optional: trigger other things when sound plays

    void Start()
    {
        if (playOnStart)
            PlaySFX();
    }

    // 🟢 Call this from UI Button, Animation Event, or Script
    public void PlaySFX()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(sfxClip);
            onPlaySFX?.Invoke(); // run extra events if assigned
        }
        else
        {
            Debug.LogWarning("SoundManager not found in scene!");
        }
    }
}
