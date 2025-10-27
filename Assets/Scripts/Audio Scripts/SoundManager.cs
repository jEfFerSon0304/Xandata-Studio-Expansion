using UnityEngine;
using System.Collections;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Default Clips (optional)")]
    public AudioClip defaultMusic;
    public AudioClip defaultSFX;

    void Awake()
    {
        // Singleton so it persists across all scenes
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 🎵 Play background music (with fade optional)
    public void PlayMusic(AudioClip clip, bool loop = true, float fadeDuration = 1f)
    {
        if (clip == null) clip = defaultMusic;
        if (musicSource.clip == clip && musicSource.isPlaying) return;

        StartCoroutine(FadeInMusic(clip, loop, fadeDuration));
    }

    // 🔇 Stop current music smoothly
    public void StopMusic(float fadeDuration = 1f)
    {
        StartCoroutine(FadeOutMusic(fadeDuration));
    }

    // 🔊 Play SFX one-shot (like button, card flip, etc.)
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) clip = defaultSFX;
        if (clip != null)
            sfxSource.PlayOneShot(clip);
    }

    // 🎚️ Volume Controls
    public void SetMusicVolume(float volume)
    {
        musicSource.volume = volume;
    }

    public void SetSFXVolume(float volume)
    {
        sfxSource.volume = volume;
    }

    // 🔄 FADE IN MUSIC
    IEnumerator FadeInMusic(AudioClip newClip, bool loop, float duration)
    {
        if (musicSource.isPlaying)
            yield return StartCoroutine(FadeOutMusic(duration));

        musicSource.clip = newClip;
        musicSource.loop = loop;
        musicSource.volume = 0f;
        musicSource.Play();

        float targetVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        float t = 0;
        while (t < duration)
        {
            musicSource.volume = Mathf.Lerp(0f, targetVolume, t / duration);
            t += Time.unscaledDeltaTime;
            yield return null;
        }
        musicSource.volume = targetVolume;
    }

    // 🔄 FADE OUT MUSIC
    IEnumerator FadeOutMusic(float duration)
    {
        float startVolume = musicSource.volume;
        float t = 0;
        while (t < duration)
        {
            musicSource.volume = Mathf.Lerp(startVolume, 0f, t / duration);
            t += Time.unscaledDeltaTime;
            yield return null;
        }
        musicSource.Stop();
        musicSource.volume = startVolume;
    }
}
