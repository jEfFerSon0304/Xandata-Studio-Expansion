using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EnergyIconGlow : MonoBehaviour
{
    [Header("UI Reference")]
    public Image energyIcon;        // Assign the Image component directly
    [Header("Pop Settings")]
    public float popDuration = 0.3f;
    public float scaleUp = 1.5f;
    [Header("Pulse Settings")]
    public float pulseSpeed = 2f;      // Speed of pulsing
    public float pulseAmount = 0.2f;   // Scale amount of pulsing
    public Color glowColor = Color.yellow; // Glow color

    private Vector3 originalScale;
    private Color originalColor;
    private Coroutine pulseCoroutine;

    private void Awake()
    {
        if (!energyIcon)
        {
            Debug.LogError("EnergyIconGlow: Assign the Image in the Inspector!");
            return;
        }

        // Save original values
        originalScale = Vector3.one;
        originalColor = energyIcon.color;

        // Start hidden
        energyIcon.transform.localScale = Vector3.zero;
        Color c = originalColor;
        c.a = 0f;
        energyIcon.color = c;
    }

    public void ShowIcon()
    {
        // Reset scale and color
        energyIcon.transform.localScale = Vector3.zero;
        Color c = originalColor;
        c.a = 0f;
        energyIcon.color = c;

        // Stop any previous pulsing
        if (pulseCoroutine != null)
            StopCoroutine(pulseCoroutine);

        // Start pop-in animation
        StartCoroutine(PopAndFadeIn());
    }

    private IEnumerator PopAndFadeIn()
    {
        float t = 0f;
        Color c = energyIcon.color;

        // Pop + fade-in
        while (t < popDuration)
        {
            t += Time.deltaTime;
            float factor = t / popDuration;

            energyIcon.transform.localScale = Vector3.Lerp(Vector3.zero, originalScale * scaleUp, factor);
            c.a = Mathf.Lerp(0f, 1f, factor);
            energyIcon.color = c;

            yield return null;
        }

        // Scale back to normal
        t = 0f;
        while (t < popDuration)
        {
            t += Time.deltaTime;
            float factor = t / popDuration;

            energyIcon.transform.localScale = Vector3.Lerp(originalScale * scaleUp, originalScale, factor);
            yield return null;
        }

        // Finalize
        energyIcon.transform.localScale = originalScale;
        energyIcon.color = originalColor;

        // Start pulsing
        pulseCoroutine = StartCoroutine(PulseGlow());
    }

    private IEnumerator PulseGlow()
    {
        float timer = 0f;
        while (true)
        {
            // Scale pulse
            float scale = 1 + Mathf.Sin(timer * pulseSpeed) * pulseAmount;
            energyIcon.transform.localScale = originalScale * scale;

            // Color pulse
            float glowFactor = (Mathf.Sin(timer * pulseSpeed * 0.5f) + 1f) / 2f; // 0 → 1
            energyIcon.color = Color.Lerp(originalColor, glowColor, glowFactor);

            timer += Time.deltaTime;
            yield return null;
        }
    }
}
