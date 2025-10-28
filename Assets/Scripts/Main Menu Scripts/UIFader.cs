using UnityEngine;
using System.Collections;

public class UIFader : MonoBehaviour
{
    public CanvasGroup[] groups;
    public float fadeDuration = 1.2f;
    public float delayBetween = 0.3f;

    void Start()
    {
        StartCoroutine(FadeSequence());
    }

    IEnumerator FadeSequence()
    {
        for (int i = 0; i < groups.Length; i++)
        {
            CanvasGroup g = groups[i];
            g.alpha = 0;
        }

        yield return new WaitForSeconds(0.2f);

        for (int i = 0; i < groups.Length; i++)
        {
            CanvasGroup g = groups[i];
            float t = 0;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                g.alpha = Mathf.Lerp(0, 1, t / fadeDuration);
                yield return null;
            }
            g.alpha = 1;
            yield return new WaitForSeconds(delayBetween);
        }
    }
}
