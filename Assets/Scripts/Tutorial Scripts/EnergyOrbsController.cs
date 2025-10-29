using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EnergyOrbsController : MonoBehaviour
{
    [Header("UI References")]
    public Image[] energyOrbs; // Assign 3 Images

    [Header("Swoop Settings")]
    public float swoopHeight = 200f;
    public float swoopDuration = 0.6f;
    public float delayBetweenOrbs = 0.1f; // optional stagger

    [Header("Glow Settings")]
    public Color glowColor = Color.yellow;
    public float glowSpeed = 2f;

    private Color[] originalColors;

    private void Awake()
    {
        originalColors = new Color[energyOrbs.Length];

        for (int i = 0; i < energyOrbs.Length; i++)
        {
            originalColors[i] = energyOrbs[i].color;

            if (!energyOrbs[i].GetComponent<CanvasGroup>())
                energyOrbs[i].gameObject.AddComponent<CanvasGroup>();

            energyOrbs[i].gameObject.SetActive(true);
        }
    }

    public void ShowOrbs()
    {
        StartCoroutine(SwoopAllOrbs());
    }

    private IEnumerator SwoopAllOrbs()
    {
        Vector3[] startPos = new Vector3[energyOrbs.Length];
        Vector3[] targetPos = new Vector3[energyOrbs.Length];

        for (int i = 0; i < energyOrbs.Length; i++)
        {
            RectTransform rt = energyOrbs[i].GetComponent<RectTransform>();
            targetPos[i] = rt.localPosition;
            startPos[i] = targetPos[i] + Vector3.up * swoopHeight;

            rt.localPosition = startPos[i];
            rt.localScale = Vector3.zero;

            CanvasGroup cg = energyOrbs[i].GetComponent<CanvasGroup>();
            cg.alpha = 0f;
        }

        for (int i = 0; i < energyOrbs.Length; i++)
        {
            StartCoroutine(SwoopOrb(energyOrbs[i], startPos[i], targetPos[i], i));
            yield return new WaitForSeconds(delayBetweenOrbs); // stagger each orb
        }
    }

    private IEnumerator SwoopOrb(Image orb, Vector3 startPos, Vector3 targetPos, int index)
    {
        float elapsed = 0f;
        RectTransform rt = orb.GetComponent<RectTransform>();
        CanvasGroup cg = orb.GetComponent<CanvasGroup>();

        while (elapsed < swoopDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / swoopDuration);

            // Cartoony jump: overshoot a little, then settle
            float height = Mathf.Sin(t * Mathf.PI) * swoopHeight; // smooth arc
            rt.localPosition = Vector3.Lerp(startPos, targetPos, Mathf.SmoothStep(0, 1, t)) + new Vector3(0, height, 0);

            // Scale with slight overshoot
            float scaleT = Mathf.SmoothStep(0, 1.2f, t); // overshoot to 1.2 then shrink to 1
            if (scaleT > 1f) scaleT = Mathf.Lerp(1.2f, 1f, (t - 0.7f) / 0.3f);
            rt.localScale = Vector3.one * scaleT;

            cg.alpha = Mathf.Lerp(0f, 1f, t);

            yield return null;
        }

        rt.localPosition = targetPos;
        rt.localScale = Vector3.one;
        cg.alpha = 1f;

        StartCoroutine(GlowOrb(orb, index));
    }


    private IEnumerator GlowOrb(Image orb, int index)
    {
        float timer = 0f;
        while (true)
        {
            float glowFactor = (Mathf.Sin(timer * glowSpeed) + 1f) / 2f;
            orb.color = Color.Lerp(originalColors[index], glowColor, glowFactor);
            timer += Time.deltaTime;
            yield return null;
        }
    }

    public void FadeOutOrbs(float duration = 0.5f)
    {
        foreach (var orb in energyOrbs)
        {
            StartCoroutine(FadeOutOrb(orb, duration));
        }
    }

    private IEnumerator FadeOutOrb(Image orb, float duration)
    {
        CanvasGroup cg = orb.GetComponent<CanvasGroup>();
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
