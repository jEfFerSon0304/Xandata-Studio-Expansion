using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.InputSystem; // For new Input System (optional)

public class SplashController : MonoBehaviour
{
    [Header("UI References")]
    public Image logoImage;
    public TMP_Text reminderText;
    public Image fadeOverlay;  // Black overlay for smooth transition

    [Header("Settings")]
    public float fadeDuration = 1.5f;
    public float logoHoldTime = 1.5f;
    public float reminderHoldTime = 2f;
    public string nextSceneName = "LoadingScreen_Landscape";

    private bool skipRequested = false;

    void Start()
    {
        // Initial alpha setup
        if (logoImage != null) SetAlpha(logoImage, 0f);
        if (reminderText != null)
        {
            reminderText.gameObject.SetActive(false);
            SetAlpha(reminderText, 0f);
        }
        if (fadeOverlay != null) SetAlpha(fadeOverlay, 0f);

        StartCoroutine(PlaySequence());
    }

    void Update()
    {
        // Detect skip input: touch or mouse click
        if (!skipRequested)
        {
            if (Input.GetMouseButtonDown(0) ||
                (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame))
            {
                skipRequested = true;
            }
        }
    }

    IEnumerator PlaySequence()
    {
        // --- Fade in logo ---
        if (logoImage != null && !skipRequested)
            yield return StartCoroutine(FadeGraphic(logoImage, 0f, 1f, fadeDuration));

        if (!skipRequested)
            yield return new WaitForSeconds(logoHoldTime);

        // --- Fade out logo ---
        if (logoImage != null && !skipRequested)
            yield return StartCoroutine(FadeGraphic(logoImage, 1f, 0f, fadeDuration));

        // --- Show and fade in reminder text ---
        if (reminderText != null && !skipRequested)
        {
            reminderText.gameObject.SetActive(true);
            yield return StartCoroutine(FadeTMPText(reminderText, 0f, 1f, fadeDuration));
        }

        if (!skipRequested)
            yield return new WaitForSeconds(reminderHoldTime);

        // --- Fade overlay to black before loading ---
        if (fadeOverlay != null)
            yield return StartCoroutine(FadeGraphic(fadeOverlay, 0f, 1f, fadeDuration));

        // --- Async load next scene ---
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(nextSceneName);
        asyncLoad.allowSceneActivation = false;

        // Optional tiny delay to ensure overlay fully black
        yield return new WaitForSeconds(0.05f);

        asyncLoad.allowSceneActivation = true;
    }

    // Generic fade for UI Graphics
    IEnumerator FadeGraphic(Graphic g, float from, float to, float duration)
    {
        float t = 0f;
        SetAlpha(g, from);
        while (t < duration && !skipRequested)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(from, to, t / duration);
            SetAlpha(g, a);
            yield return null;
        }
        SetAlpha(g, to);
    }

    // Fade for TMP_Text
    IEnumerator FadeTMPText(TMP_Text txt, float from, float to, float duration)
    {
        float t = 0f;
        SetAlpha(txt, from);
        while (t < duration && !skipRequested)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(from, to, t / duration);
            SetAlpha(txt, a);
            yield return null;
        }
        SetAlpha(txt, to);
    }

    void SetAlpha(Graphic g, float alpha)
    {
        if (g == null) return;
        Color c = g.color;
        c.a = alpha;
        g.color = c;
    }

    void SetAlpha(TMP_Text t, float alpha)
    {
        if (t == null) return;
        Color c = t.color;
        c.a = alpha;
        t.color = c;
    }
}
