using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

        // Start particle system if assigned
        if (starTrail != null)
            starTrail.Play();
    }

    void Update()
    {
        if (isLoading)
        {
            // Random progress bumps
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

            // Smooth progress
            currentProgress = Mathf.Lerp(currentProgress, targetProgress, Time.deltaTime * 4f);

            // Update visuals
            loadingBarFill.fillAmount = currentProgress;
            loadingText.text = "Loading... " + Mathf.RoundToInt(currentProgress * 100) + "%";

            // --- Move Star Knob ---
            float barWidth = fillRect.rect.width;
            float filledWidth = barWidth * currentProgress;
            float halfStarWidth = knobRect.rect.width * 0.5f;
            knobRect.anchoredPosition = new Vector2(filledWidth - halfStarWidth + positionOffset, 0);

            // --- Ensure particle trail plays ---
            if (starTrail != null && !starTrail.isPlaying)
            {
                starTrail.Play();
            }

            // --- Loading complete ---
            if (currentProgress >= 0.995f)
            {
                isLoading = false;
                loadingText.text = "Tap to Continue";
            }
        }
        else
        {
            // Tap-to-continue for mobile and editor
            if (Mouse.current.leftButton.wasPressedThisFrame ||
                (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame))
            {
                SceneManager.LoadScene(nextSceneName);
            }
        }
    }
}
