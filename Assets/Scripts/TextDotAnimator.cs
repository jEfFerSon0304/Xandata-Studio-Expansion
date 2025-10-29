using System.Collections;
using TMPro;
using UnityEngine;

public class TextDotAnimator : MonoBehaviour
{
    public TMP_Text textUI;
    public string baseText = "Waiting";
    public float interval = 0.4f;

    private Coroutine loop;

    void OnEnable()
    {
        StartAnimation();
    }

    void OnDisable()
    {
        StopAnimation();
    }

    public void StartAnimation()
    {
        if (loop == null)
            loop = StartCoroutine(DotLoop());
    }

    public void StopAnimation()
    {
        if (loop != null)
        {
            StopCoroutine(loop);
            loop = null;
            textUI.text = baseText;
        }
    }

    IEnumerator DotLoop()
    {
        int dotCount = 0;
        while (true)
        {
            textUI.text = baseText + new string('.', dotCount);
            dotCount = (dotCount + 1) % 4;
            yield return new WaitForSeconds(interval);
        }
    }
}
