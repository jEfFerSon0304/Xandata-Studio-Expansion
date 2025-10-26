using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class ReminderController : MonoBehaviour
{
    public TextMeshProUGUI reminderText;
    public Image fadeOverlay;
    public float fadeDuration = 1.2f;
    public float displayDuration = 2.5f;

    private void Start()
    {
        SetAlpha(reminderText, 0f);
        SetAlpha(fadeOverlay, 1f);

        StartCoroutine(PlaySequence());
    }

    IEnumerator PlaySequence()
    {
        // Fade from black to reveal background
        yield return StartCoroutine(FadeOut(fadeOverlay));

        // Text fades in
        yield return StartCoroutine(FadeIn(reminderText));
        yield return new WaitForSeconds(displayDuration);

        // Text fades out
        yield return StartCoroutine(FadeOut(reminderText));

        // Fade to black before loading next scene
        yield return StartCoroutine(FadeIn(fadeOverlay));

        SceneManager.LoadScene("LoadingScene");
    }

    IEnumerator FadeIn(Graphic g)
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            SetAlpha(g, Mathf.Lerp(0f, 1f, elapsed / fadeDuration));
            yield return null;
        }
        SetAlpha(g, 1f);
    }

    IEnumerator FadeOut(Graphic g)
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            SetAlpha(g, Mathf.Lerp(1f, 0f, elapsed / fadeDuration));
            yield return null;
        }
        SetAlpha(g, 0f);
    }

    void SetAlpha(Graphic g, float alpha)
    {
        if (g == null) return;
        Color c = g.color;
        c.a = alpha;
        g.color = c;
    }
}
