using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class VictoryManager : MonoBehaviour
{
    [Header("Scene References")]
    public Animator victoryAnimator;
    public Button mainMenuButton;

    [Header("Sound Effects")]
    public AudioClip victorySFX;   // drag your victory SFX here
    public AudioClip clickSFX;     // drag your button click SFX here

    void Start()
    {
        // Hide button at start
        if (mainMenuButton != null)
            mainMenuButton.gameObject.SetActive(false);

        // Play victory sound via SoundManager
        if (SoundManager.Instance != null && victorySFX != null)
            SoundManager.Instance.PlaySFX(victorySFX);

        // Play victory animation (if assigned)
        if (victoryAnimator != null)
            victoryAnimator.Play("VictoryAnim");

        // Show button after a short delay
        Invoke(nameof(ShowMainMenuButton), 2.5f);
    }

    void ShowMainMenuButton()
    {
        if (mainMenuButton != null)
            mainMenuButton.gameObject.SetActive(true);
    }

    public void LoadMainMenu()
    {
        if (SoundManager.Instance != null)
        {
            // Stop the current SFX (victory fanfare)
            SoundManager.Instance.sfxSource.Stop();

            // Play click sound
            SoundManager.Instance.PlaySFX(clickSFX);
        }

        // Load Main Menu scene
        SceneManager.LoadScene("MainMenu");
    }
}
