using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class StarsController : MonoBehaviour
{
    [Header("UI References")]
    public Image[] stars; // Assign 3 star Images

    [Header("Swoop Settings")]
    public float swoopHeight = 200f;
    public float swoopDuration = 0.6f;
    public float delayBetweenStars = 0.1f;

    [Header("Glow Settings")]
    public Color glowColor = Color.yellow;
    public float glowSpeed = 2f;

    private Color[] originalColors;

    private void Awake()
    {
        originalColors = new Color[stars.Length];

        for (int i = 0; i < stars.Length; i++)
        {
            originalColors[i] = stars[i].color;

            if (!stars[i].GetComponent<CanvasGroup>())
                stars[i].gameObject.AddComponent<CanvasGroup>();

            stars[i].gameObject.SetActive(true);
        }
    }

    public void ShowStars()
    {
        StartCoroutine(SwoopAllStars());
    }

    private IEnumerator SwoopAllStars()
    {
        Vector3[] startPos = new Vector3[stars.Length];
        Vector3[] targetPos = new Vector3[stars.Length];

        for (int i = 0; i < stars.Length; i++)
        {
            RectTransform rt = stars[i].GetComponent<RectTransform>();
            targetPos[i] = rt.localPosition;
            startPos[i] = targetPos[i] + Vector3.up * swoopHeight;

            rt.localPosition = startPos[i];
            rt.localScale = Vector3.zero;

            CanvasGroup cg = stars[i].GetComponent<CanvasGroup>();
            cg.alpha = 0f;
        }

        for (int i = 0; i < stars.Length; i++)
        {
            StartCoroutine(SwoopStar(stars[i], startPos[i], targetPos[i], i));
            yield return new WaitForSeconds(delayBetweenStars);
        }
    }

    private IEnumerator SwoopStar(Image star, Vector3 startPos, Vector3 targetPos, int index)
    {
        float elapsed = 0f;
        RectTransform rt = star.GetComponent<RectTransform>();
        CanvasGroup cg = star.GetComponent<CanvasGroup>();

        while (elapsed < swoopDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / swoopDuration);

            float height = Mathf.Sin(t * Mathf.PI) * swoopHeight;
            rt.localPosition = Vector3.Lerp(startPos, targetPos, Mathf.SmoothStep(0, 1, t)) + new Vector3(0, height, 0);

            float scaleT = Mathf.SmoothStep(0, 1.2f, t);
            if (scaleT > 1f) scaleT = Mathf.Lerp(1.2f, 1f, (t - 0.7f) / 0.3f);
            rt.localScale = Vector3.one * scaleT;

            cg.alpha = Mathf.Lerp(0f, 1f, t);

            yield return null;
        }

        rt.localPosition = targetPos;
        rt.localScale = Vector3.one;
        cg.alpha = 1f;

        StartCoroutine(GlowStar(star, index));
    }

    private IEnumerator GlowStar(Image star, int index)
    {
        float timer = 0f;
        while (true)
        {
            float glowFactor = (Mathf.Sin(timer * glowSpeed) + 1f) / 2f;
            star.color = Color.Lerp(originalColors[index], glowColor, glowFactor);
            timer += Time.deltaTime;
            yield return null;
        }
    }

    public void FadeOutStars(float duration = 0.5f)
    {
        foreach (var star in stars)
        {
            StartCoroutine(FadeOutStar(star, duration));
        }
    }

    private IEnumerator FadeOutStar(Image star, float duration)
    {
        CanvasGroup cg = star.GetComponent<CanvasGroup>();
        if (cg == null) yield break;

        float startAlpha = cg.alpha;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(startAlpha, 0f, t / duration);
            yield return null;
        }
        cg.alpha = 0f;
    }
}
