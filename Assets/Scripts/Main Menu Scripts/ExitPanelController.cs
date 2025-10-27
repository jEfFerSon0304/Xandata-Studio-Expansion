using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ExitPanelController : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public Button yesButton;
    public Button noButton;
    public Button backgroundButton; // for clicking outside the modal

    private bool isVisible = false;

    void Start()
    {
        yesButton.onClick.AddListener(QuitGame);
        noButton.onClick.AddListener(HidePanel);
        backgroundButton.onClick.AddListener(HidePanel);
    }

    public void ShowPanel()
    {
        if (isVisible) return;
        isVisible = true;

        StartCoroutine(FadeCanvasGroup(canvasGroup, 0, 1, 0.25f));
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    public void HidePanel()
    {
        if (!isVisible) return;
        isVisible = false;

        StartCoroutine(FadeCanvasGroup(canvasGroup, 1, 0, 0.25f));
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float start, float end, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Lerp(start, end, elapsed / duration);
            yield return null;
        }
        cg.alpha = end;
    }

    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
