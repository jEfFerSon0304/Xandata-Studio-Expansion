using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class PlaySFXTrigger : MonoBehaviour
{
    [Header("Sound Settings")]
    public AudioClip sfxClip;             // Assign your sound here in Inspector
    public bool playOnStart = false;      // Optional: play automatically when scene starts
    public float delay = 0f;              // Optional delay in seconds before playing

    [Header("Optional Events")]
    public UnityEvent onPlaySFX;          // Optional: trigger other things when sound plays

    private Coroutine playCoroutine;

    void Start()
    {
        if (playOnStart)
            playCoroutine = StartCoroutine(PlaySFXWithDelay());
    }

    public void PlaySFX()
    {
        if (SoundManager.Instance != null && sfxClip != null)
        {
            // Stop previous sfx if still playing
            SoundManager.Instance.sfxSource.Stop();
            SoundManager.Instance.sfxSource.clip = sfxClip;
            SoundManager.Instance.sfxSource.Play();
            onPlaySFX?.Invoke();
        }
        else
        {
            Debug.LogWarning("SoundManager or sfxClip not found!");
        }
    }

    private IEnumerator PlaySFXWithDelay()
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        PlaySFX();
    }

    public void StopSFX()
    {
        if (playCoroutine != null)
            StopCoroutine(playCoroutine);
        if (SoundManager.Instance != null)
            SoundManager.Instance.sfxSource.Stop();
    }
}
