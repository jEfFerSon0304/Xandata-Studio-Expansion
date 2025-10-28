using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem; // New Input System support

public class LoadingScreen : MonoBehaviour
{
    [Header("UI References")]
    public Image loadingBarFill;
    public Image starKnob;
    public TextMeshProUGUI loadingText;
    public ParticleSystem starTrail; // Particle system for knob

    [Header("Settings")]
    public float loadTime = 3.5f;
    public string nextSceneName = "MainMenu";

    private float currentProgress = 0f;
    private float targetProgress = 0f;
    private RectTransform knobRect;
    private RectTransform fillRect;
    private bool isLoading = true;
    private float stutterTimer = 0f;

    private const float positionOffset = 20f; // keeps your current alignment

    void Start()
    {
        knobRect = starKnob.GetComponent<RectTransform>();
        fillRect = loadingBarFill.GetComponent<RectTransform>();

        loadingBarFill.fillAmount = 0f;
        loadingText.text = "Loading... 0%";

        if (starTrail != null)
            starTrail.Play();
    }

    void Update()
    {
        if (isLoading)
        {
            // Simulate loading progress
            if (stutterTimer <= 0f)
            {
                float randomJump = Random.Range(0.03f, 0.07f);
                targetProgress = Mathf.Min(targetProgress + randomJump, 1f);
                stutterTimer = Random.Range(0.15f, 0.3f);
            }
            else
            {
                stutterTimer -= Time.deltaTime;
            }

            currentProgress = Mathf.Lerp(currentProgress, targetProgress, Time.deltaTime * 4f);

            // Update visuals
            loadingBarFill.fillAmount = currentProgress;
            loadingText.text = "Loading... " + Mathf.RoundToInt(currentProgress * 100) + "%";

            // Move the star knob smoothly along the bar
            float barWidth = fillRect.rect.width;
            float filledWidth = barWidth * currentProgress;
            float halfStarWidth = knobRect.rect.width * 0.5f;
            knobRect.anchoredPosition = new Vector2(filledWidth - halfStarWidth + positionOffset, 0);

            // Keep particle trail playing
            if (starTrail != null && !starTrail.isPlaying)
                starTrail.Play();

            // Finish loading
            if (currentProgress >= 0.995f)
            {
                isLoading = false;
                loadingText.text = "Tap to Continue";
            }
        }
        else
        {
            // ✅ Works on both PC (mouse) and Android (touch)
            bool tapped = false;

            // Touch input (Android)
            if (Touchscreen.current != null &&
                Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            {
                tapped = true;
            }

            // Mouse input (Editor/PC)
            if (Mouse.current != null &&
                Mouse.current.leftButton.wasPressedThisFrame)
            {
                tapped = true;
            }

            if (tapped)
            {
                SceneManager.LoadScene(nextSceneName);
            }
        }
    }
}
