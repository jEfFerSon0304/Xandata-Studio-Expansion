using System.Collections;
using TMPro;
using UnityEngine;

[System.Serializable]
public class SkillTextAnimator
{
    public enum TextAnimStyle { Fade, Slide, Pop, Shake, Wave, Glow }

    [Header("Animation Settings")]
    public TextAnimStyle style = TextAnimStyle.Fade;
    public float duration = 0.8f;
    public float slideDistance = 100f;
    public float shakeIntensity = 8f;
    public float waveHeight = 8f;
    public float waveSpeed = 3f;

    // 🌟 Fade animation
    public IEnumerator FadeIn(TextMeshProUGUI text)
    {
        float t = 0f;
        text.alpha = 0;
        while (t < duration)
        {
            t += Time.deltaTime;
            text.alpha = Mathf.SmoothStep(0, 1, t / duration);
            yield return null;
        }
        text.alpha = 1;
    }

    // 🌪️ Slide-in from below
    public IEnumerator SlideIn(TextMeshProUGUI text)
    {
        Vector3 startPos = text.rectTransform.localPosition - new Vector3(0, slideDistance, 0);
        Vector3 endPos = text.rectTransform.localPosition;

        float t = 0f;
        text.alpha = 0;
        while (t < duration)
        {
            t += Time.deltaTime;
            float p = Mathf.SmoothStep(0, 1, t / duration);
            text.alpha = p;
            text.rectTransform.localPosition = Vector3.Lerp(startPos, endPos, p);
            yield return null;
        }
        text.rectTransform.localPosition = endPos;
        text.alpha = 1;
    }

    // 💥 Pop / scale animation
    public IEnumerator PopIn(TextMeshProUGUI text)
    {
        Vector3 originalScale = text.rectTransform.localScale;
        text.alpha = 0;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float p = Mathf.SmoothStep(0, 1, t / duration);
            float scale = Mathf.Lerp(0.8f, 1.2f, Mathf.Sin(p * Mathf.PI));
            text.rectTransform.localScale = originalScale * scale;
            text.alpha = p;
            yield return null;
        }

        text.rectTransform.localScale = originalScale;
        text.alpha = 1;
    }

    // ⚡ Shake animation
    public IEnumerator ShakeIn(TextMeshProUGUI text)
    {
        Vector3 originalPos = text.rectTransform.localPosition;
        float t = 0f;
        text.alpha = 1;

        while (t < duration)
        {
            t += Time.deltaTime;
            float p = 1 - (t / duration);
            Vector3 offset = new Vector3(
                Random.Range(-shakeIntensity * p, shakeIntensity * p),
                Random.Range(-shakeIntensity * p, shakeIntensity * p),
                0f
            );
            text.rectTransform.localPosition = originalPos + offset;
            yield return null;
        }

        text.rectTransform.localPosition = originalPos;
    }

    // 🌊 Wave animation (floaty motion)
    public IEnumerator WaveIn(TextMeshProUGUI text)
    {
        Vector3 startPos = text.rectTransform.localPosition;
        float t = 0f;
        text.alpha = 0;

        while (t < duration)
        {
            t += Time.deltaTime;
            float p = t / duration;
            text.alpha = Mathf.SmoothStep(0, 1, p);

            float offsetY = Mathf.Sin(Time.time * waveSpeed) * waveHeight;
            text.rectTransform.localPosition = startPos + new Vector3(0, offsetY, 0);
            yield return null;
        }

        text.rectTransform.localPosition = startPos;
        text.alpha = 1;
    }

    // 🔥 Glow effect (if using TextMeshPro material with glow)
    public IEnumerator GlowIn(TextMeshProUGUI text)
    {
        Material mat = text.fontMaterial;
        float t = 0f;
        text.alpha = 1;

        while (t < duration)
        {
            t += Time.deltaTime;
            float glow = Mathf.Lerp(4f, 0.5f, t / duration);
            mat.SetFloat("_GlowPower", glow);
            yield return null;
        }
    }

    // ✨ Automatically run based on enum
    public IEnumerator Play(TextMeshProUGUI text)
    {
        switch (style)
        {
            case TextAnimStyle.Slide: yield return SlideIn(text); break;
            case TextAnimStyle.Pop: yield return PopIn(text); break;
            case TextAnimStyle.Shake: yield return ShakeIn(text); break;
            case TextAnimStyle.Wave: yield return WaveIn(text); break;
            case TextAnimStyle.Glow: yield return GlowIn(text); break;
            default: yield return FadeIn(text); break;
        }
    }
}
