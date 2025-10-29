using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SkillRevealUI : MonoBehaviour
{
    [Header("UI References")]
    public CanvasGroup background;
    public Image backgroundImage;
    public Image mainImage;
    public TextMeshProUGUI skillNameText;
    public TextMeshProUGUI skillDescText;
    public TextMeshProUGUI tapToContinueText;

    private Vector3 originalBgPos;
    private bool waitingForTap;

    private void Awake()
    {
        if (backgroundImage != null)
            originalBgPos = backgroundImage.rectTransform.localPosition;
    }

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
        if (anim == null) yield break;

        waitingForTap = false;
        background.gameObject.SetActive(true);
        tapToContinueText.gameObject.SetActive(false);

        // 🧹 Reset visuals
        background.alpha = 0;
        mainImage.color = new Color(1, 1, 1, 0);
        skillNameText.alpha = 0;
        skillDescText.alpha = 0;
        tapToContinueText.alpha = 0f;
        tapToContinueText.gameObject.SetActive(false);

        // Setup visuals
        backgroundImage.sprite = anim.backgroundImage;
        mainImage.sprite = anim.mainImage;
        backgroundImage.color = anim.backgroundTint;
        skillNameText.text = data.skillName;
        skillDescText.text = data.description;

        // 1️⃣ Fade in background
        float t = 0;
        while (t < anim.fadeDuration)
        {
            t += Time.deltaTime;
            background.alpha = Mathf.Lerp(0, 1, t / anim.fadeDuration);
            //ApplyShake(anim.shakeIntensity * 0.5f);
            yield return null;
        }

        // 2️⃣ Delay then show main image
        yield return new WaitForSeconds(anim.mainImageDelay);
        t = 0;
        while (t < 0.3f)
        {
            t += Time.deltaTime;
            float a = t / 0.3f;
            mainImage.color = new Color(1, 1, 1, a);
            //ApplyShake(anim.shakeIntensity, anim.shakeMainImage);
            yield return null;
        }

        // 3️⃣ Play SFX
        if (anim.skillSFX)
            AudioSource.PlayClipAtPoint(anim.skillSFX, Camera.main.transform.position);

        StartCoroutine(ShakeRoutine(anim.shakeIntensity, anim.shakeDuration, anim.shakeMainImage));


        // 4️⃣ Fade in text
        yield return new WaitForSeconds(anim.textFadeInDelay);
        t = 0;
        while (t < anim.textFadeInDuration)
        {
            t += Time.deltaTime;
            float a = t / anim.textFadeInDuration;
            skillNameText.alpha = a;
            skillDescText.alpha = a;
            //ApplyShake(anim.shakeIntensity * 0.3f);
            yield return null;
        }

        // 5️⃣ Optional “Tap to continue”
        yield return new WaitForSeconds(anim.textStayDuration);
        if (anim.showTapToContinue)
        {
            tapToContinueText.gameObject.SetActive(true);
            waitingForTap = true;
        }

        // 6️⃣ Wait for player tap (new Input System)
        yield return new WaitUntil(() => Mouse.current.leftButton.wasPressedThisFrame);

        // 7️⃣ Fade out everything smoothly
        float fadeOutTime = 0.4f;
        float b = 0;
        while (b < fadeOutTime)
        {
            b += Time.deltaTime;
            float a = 1 - (b / fadeOutTime);
            background.alpha = a;
            mainImage.color = new Color(1, 1, 1, a);
            skillNameText.alpha = a;
            skillDescText.alpha = a;
            tapToContinueText.alpha = a;
            //ApplyShake(anim.shakeIntensity * 0.4f);
            yield return null;
        }

        // ✅ Reset everything (ready for next skill)
        background.alpha = 0;
        mainImage.color = new Color(1, 1, 1, 0);
        skillNameText.alpha = 0;
        skillDescText.alpha = 0;
        tapToContinueText.alpha = 0;
        tapToContinueText.gameObject.SetActive(false);
        backgroundImage.rectTransform.localPosition = originalBgPos;
        mainImage.rectTransform.localPosition = Vector3.zero;
    }

    //private void ApplyShake(float intensity, bool shakeMainImage = true)
    //{
    //    if (intensity <= 0f) return;

    //    Vector3 offset = new Vector3(
    //        Random.Range(-intensity, intensity),
    //        Random.Range(-intensity, intensity),
    //        0f
    //    );

    //    // 🌀 Shake the main image for a more dynamic effect
    //    if (shakeMainImage && mainImage != null)
    //        mainImage.rectTransform.localPosition = originalBgPos + offset;
    //    else if (backgroundImage != null)
    //        backgroundImage.rectTransform.localPosition = originalBgPos + offset;
    //}

    private IEnumerator ShakeRoutine(float intensity, float duration, bool shakeMainImage)
    {
        if (intensity <= 0f || duration <= 0f) yield break;

        float timer = 0f;
        Vector3 mainOriginal = mainImage.rectTransform.localPosition;
        Vector3 bgOriginal = backgroundImage.rectTransform.localPosition;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            // 👇 Gradually reduce shake intensity as time passes
            float normalized = timer / duration;
            float currentIntensity = Mathf.Lerp(intensity, 0f, Mathf.SmoothStep(0f, 1f, normalized));

            Vector3 offset = new Vector3(
                Random.Range(-currentIntensity, currentIntensity),
                Random.Range(-currentIntensity, currentIntensity),
                0f
            );

            if (shakeMainImage && mainImage != null)
                mainImage.rectTransform.localPosition = mainOriginal + offset;
            else if (backgroundImage != null)
                backgroundImage.rectTransform.localPosition = bgOriginal + offset;

            yield return null;
        }

        // ✅ Reset positions after shaking ends
        if (mainImage != null)
            mainImage.rectTransform.localPosition = mainOriginal;
        if (backgroundImage != null)
            backgroundImage.rectTransform.localPosition = bgOriginal;
    }



}
