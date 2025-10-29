using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class SkillRevealUI : MonoBehaviour
{
    [Header("UI References")]
    public CanvasGroup background;
    public Image backgroundImage;
    public Image mainImage;
    public TextMeshProUGUI skillNameText;
    public TextMeshProUGUI skillDescText;
    public TextMeshProUGUI tapToContinueText;

    private bool waitingForTap;

    public void ShowSkill(CharacterDataSO.SkillData data)
    {
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        StopAllCoroutines();
        StartCoroutine(PlaySkillAnimation(data));
    }

    private IEnumerator PlaySkillAnimation(CharacterDataSO.SkillData data)
    {
        var anim = data.animationData;
        if (anim == null)
        {
            Debug.LogWarning($"No animation data assigned for {data.skillName}");
            yield break;
        }

        waitingForTap = false;
        background.gameObject.SetActive(true);
        tapToContinueText.gameObject.SetActive(false);

        // Setup visuals
        backgroundImage.sprite = anim.backgroundImage;
        mainImage.sprite = anim.mainImage;
        backgroundImage.color = anim.backgroundTint;

        skillNameText.text = data.skillName;
        skillDescText.text = data.description;

        // 🪄 Store defaults to restore later
        TMP_FontAsset originalFontName = skillNameText.font;
        TMP_FontAsset originalFontDesc = skillDescText.font;
        Color originalColorName = skillNameText.color;
        Color originalColorDesc = skillDescText.color;

        // Apply animation-specific font + color
        if (anim.skillFont != null)
        {
            skillNameText.font = anim.skillFont;
            skillDescText.font = anim.skillFont;
        }
        skillNameText.color = anim.skillTextColor;
        skillDescText.color = anim.skillTextColor;

        // Reset visuals
        background.alpha = 0;
        mainImage.color = new Color(1, 1, 1, 0);
        skillNameText.alpha = 0;
        skillDescText.alpha = 0;
        tapToContinueText.alpha = 0;
        tapToContinueText.gameObject.SetActive(false);

        // 🎬 Fade background
        float t = 0;
        while (t < anim.fadeDuration)
        {
            t += Time.deltaTime;
            background.alpha = Mathf.Lerp(0, 1, t / anim.fadeDuration);
            yield return null;
        }

        // 🕒 Delay before main image
        yield return new WaitForSeconds(anim.mainImageDelay);

        // Main image fade-in
        t = 0;
        while (t < 0.4f)
        {
            t += Time.deltaTime;
            float a = t / 0.4f;
            mainImage.color = new Color(1, 1, 1, a);
            yield return null;
        }

        // 🔊 Play SFX if available
        if (anim.skillSFX)
            AudioSource.PlayClipAtPoint(anim.skillSFX, Camera.main.transform.position);

        // 🎥 Shake effect
        if (anim.shakeMainImage)
            StartCoroutine(ShakeObject(mainImage.rectTransform, anim.shakeIntensity, anim.shakeDuration));

        // ✨ Text Animation
        yield return new WaitForSeconds(anim.textFadeInDelay);
        yield return AnimateSkillText(anim.textEffect, anim.textFadeInDuration, anim.textMoveDistance, anim.textScaleFactor, anim.customCurve);

        // Wait before showing Tap to Continue
        yield return new WaitForSeconds(anim.textStayDuration);
        if (anim.showTapToContinue)
        {
            tapToContinueText.gameObject.SetActive(true);
            waitingForTap = true;
        }

        // Wait for player input
        yield return new WaitUntil(() => Mouse.current.leftButton.wasPressedThisFrame);

        // 🌀 Fade out everything
        t = 0;
        while (t < 0.4f)
        {
            t += Time.deltaTime;
            float a = 1 - (t / 0.4f);
            background.alpha = a;
            mainImage.color = new Color(1, 1, 1, a);
            skillNameText.alpha = a;
            skillDescText.alpha = a;
            tapToContinueText.alpha = a;
            yield return null;
        }

        // 🔁 Reset
        background.alpha = 0;
        mainImage.color = new Color(1, 1, 1, 0);
        skillNameText.alpha = 0;
        skillDescText.alpha = 0;
        tapToContinueText.alpha = 0;
        tapToContinueText.gameObject.SetActive(false);

        // Restore fonts and colors
        skillNameText.font = originalFontName;
        skillDescText.font = originalFontDesc;
        skillNameText.color = originalColorName;
        skillDescText.color = originalColorDesc;
    }

    // ----------------------------------------------------------
    // TEXT ANIMATION BEHAVIORS (Skill-specific)
    // ----------------------------------------------------------
    private IEnumerator AnimateSkillText(SkillAnimationSO.SkillTextEffectType effect, float duration, float moveDistance, float scaleFactor, AnimationCurve curve)
    {
        Vector3 startPosName = skillNameText.rectTransform.localPosition;
        Vector3 startPosDesc = skillDescText.rectTransform.localPosition;
        float t = 0f;

        switch (effect)
        {
            case SkillAnimationSO.SkillTextEffectType.FadeIn:
                while (t < duration)
                {
                    t += Time.deltaTime;
                    float a = curve.Evaluate(t / duration);
                    skillNameText.alpha = a;
                    skillDescText.alpha = a;
                    yield return null;
                }
                break;

            case SkillAnimationSO.SkillTextEffectType.SlideFromLeft:
                Vector3 left = new Vector3(-moveDistance, 0, 0);
                while (t < duration)
                {
                    t += Time.deltaTime;
                    float a = curve.Evaluate(t / duration);
                    skillNameText.rectTransform.localPosition = Vector3.Lerp(startPosName + left, startPosName, a);
                    skillDescText.rectTransform.localPosition = Vector3.Lerp(startPosDesc + left, startPosDesc, a * 0.8f);
                    skillNameText.alpha = a;
                    yield return null;
                }
                break;

            case SkillAnimationSO.SkillTextEffectType.SlideFromRight:
                Vector3 right = new Vector3(moveDistance, 0, 0);
                while (t < duration)
                {
                    t += Time.deltaTime;
                    float a = curve.Evaluate(t / duration);
                    skillNameText.rectTransform.localPosition = Vector3.Lerp(startPosName + right, startPosName, a);
                    skillDescText.rectTransform.localPosition = Vector3.Lerp(startPosDesc + right, startPosDesc, a * 0.8f);
                    skillNameText.alpha = a;
                    yield return null;
                }
                break;

            case SkillAnimationSO.SkillTextEffectType.Bounce:
                while (t < duration)
                {
                    t += Time.deltaTime;
                    float bounce = Mathf.Sin(t * Mathf.PI * 2f) * 0.2f;
                    float a = Mathf.Clamp01(t / duration);
                    skillNameText.rectTransform.localScale = Vector3.one * (1 + bounce * scaleFactor);
                    skillDescText.alpha = a;
                    skillNameText.alpha = a;
                    yield return null;
                }
                skillNameText.rectTransform.localScale = Vector3.one;
                break;

            case SkillAnimationSO.SkillTextEffectType.Wave:
                while (t < duration)
                {
                    t += Time.deltaTime;
                    float wave = Mathf.Sin(t * 6f) * 8f;
                    float a = Mathf.Clamp01(t / duration);
                    skillNameText.rectTransform.localPosition = startPosName + new Vector3(0, wave, 0);
                    skillDescText.rectTransform.localPosition = startPosDesc + new Vector3(0, wave * 0.5f, 0);
                    skillNameText.alpha = a;
                    skillDescText.alpha = a;
                    yield return null;
                }
                break;

            case SkillAnimationSO.SkillTextEffectType.Impact:
                while (t < duration)
                {
                    t += Time.deltaTime;
                    float scale = 1 + Mathf.Sin(t * Mathf.PI) * 0.4f;
                    skillNameText.rectTransform.localScale = Vector3.one * scale;
                    skillNameText.alpha = Mathf.Clamp01(t / duration);
                    yield return null;
                }
                skillNameText.rectTransform.localScale = Vector3.one;
                break;
        }

        skillNameText.rectTransform.localPosition = startPosName;
        skillDescText.rectTransform.localPosition = startPosDesc;
    }

    // ----------------------------------------------------------
    // SHAKE EFFECT
    // ----------------------------------------------------------
    private IEnumerator ShakeObject(RectTransform target, float intensity, float duration)
    {
        Vector3 originalPos = target.localPosition;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float strength = Mathf.Lerp(intensity, 0, elapsed / duration);
            target.localPosition = originalPos + Random.insideUnitSphere * strength;
            yield return null;
        }
        target.localPosition = originalPos;
    }
}
