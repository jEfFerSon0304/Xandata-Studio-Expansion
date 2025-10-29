using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class CardRevealController : MonoBehaviour
{
    [Header("UI References")]
    public CanvasGroup background;      // full-screen background
    public TextMeshProUGUI abilityText; // ability name text

    [Header("Animation Settings")]
    public float totalDuration = 6f;       // total animation duration
    public float shakeMagnitude = 20f;     // shake intensity
    public float textSlideDistance = 800f; // offscreen start/end distance
    [Range(0f, 1f)]
    public float fadeInRatio = 0.15f;      // % of total duration for fade/scale in
    [Range(0f, 1f)]
    public float textSlideInRatio = 0.1f;  // % of total duration for text slide in
    [Range(0f, 1f)]
    public float slowTextRatio = 0.6f;     // % of total duration for slow text movement
    [Range(0f, 1f)]
    public float textSlideOutRatio = 0.15f; // % of total duration for text slide out

    [Header("SFX")]
    public AudioClip revealSFX; // assign the clip via Inspector

    private Vector3 bgOriginalScale;
    private Vector3 bgOriginalPos;
    private Vector3 textOriginalPos;

    private void Awake()
    {
        // Background setup
        background.alpha = 0f;
        background.gameObject.SetActive(false);
        bgOriginalScale = background.transform.localScale;
        bgOriginalPos = background.transform.localPosition;

        // Text setup
        abilityText.gameObject.SetActive(false);
        textOriginalPos = abilityText.rectTransform.localPosition;
    }

    public IEnumerator RevealCoroutine(string abilityName = "Stampede")
    {
        // Play reveal SFX via persistent SoundManager
        if (revealSFX != null)
        {
            SoundManager.Instance.sfxSource.PlayOneShot(revealSFX);
        }

        // Calculate durations
        float fadeInDuration = totalDuration * fadeInRatio;
        float slideInDuration = totalDuration * textSlideInRatio;
        float slowTextDuration = totalDuration * slowTextRatio;
        float slideOutDuration = totalDuration * textSlideOutRatio;

        // Activate background
        background.gameObject.SetActive(true);
        background.alpha = 0f;
        background.transform.localScale = bgOriginalScale * 0.5f;

        // Set text
        abilityText.text = abilityName;
        abilityText.gameObject.SetActive(true);

        Vector3 startPos = textOriginalPos + Vector3.left * textSlideDistance;
        Vector3 slowEnd = textOriginalPos + Vector3.right * 50f; // slow drift
        Vector3 endPos = textOriginalPos + Vector3.right * textSlideDistance;
        abilityText.rectTransform.localPosition = startPos;

        // 1️⃣ Fade in + scale background
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Sin((elapsed / fadeInDuration) * Mathf.PI * 0.5f); // ease-out
            background.transform.localScale = Vector3.Lerp(bgOriginalScale * 0.5f, bgOriginalScale, t);
            background.alpha = Mathf.Lerp(0f, 1f, t);
            ShakeBackground();
            yield return null;
        }
        background.transform.localScale = bgOriginalScale;
        background.alpha = 1f;

        // 2️⃣ Text slide in
        elapsed = 0f;
        while (elapsed < slideInDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Pow(elapsed / slideInDuration, 2f); // ease-in
            abilityText.rectTransform.localPosition = Vector3.Lerp(startPos, textOriginalPos, t);
            ShakeBackground();
            yield return null;
        }
        abilityText.rectTransform.localPosition = textOriginalPos;

        // 3️⃣ Slow text drift (fast-slow-fast easing)
        elapsed = 0f;
        while (elapsed < slowTextDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / slowTextDuration;
            float easedT = Mathf.Sin(t * Mathf.PI - Mathf.PI / 2f) * 0.5f + 0.5f; // ease in-out-in
            abilityText.rectTransform.localPosition = Vector3.Lerp(textOriginalPos, slowEnd, easedT);
            ShakeBackground();
            yield return null;
        }

        // 4️⃣ Text slide out fully (ease out)
        elapsed = 0f;
        while (elapsed < slideOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Pow(elapsed / slideOutDuration, 2f); // ease-out
            abilityText.rectTransform.localPosition = Vector3.Lerp(slowEnd, endPos, t);
            ShakeBackground();
            yield return null;
        }
        abilityText.gameObject.SetActive(false);

        // 5️⃣ Fade out background AFTER text is fully gone
        elapsed = 0f;
        float startAlpha = background.alpha;
        while (elapsed < 0.5f) // fade out duration fixed to 0.5s
        {
            elapsed += Time.deltaTime;
            float t = elapsed / 0.5f;
            background.alpha = Mathf.Lerp(startAlpha, 0f, t);
            yield return null;
        }
        background.alpha = 0f;
        background.gameObject.SetActive(false);
        background.transform.localScale = bgOriginalScale;
        background.transform.localPosition = bgOriginalPos;
    }

    private void ShakeBackground()
    {
        Vector3 shakeOffset = new Vector3(
            Random.Range(-shakeMagnitude, shakeMagnitude),
            Random.Range(-shakeMagnitude, shakeMagnitude),
            0f
        );
        background.transform.localPosition = bgOriginalPos + shakeOffset;
    }
}
