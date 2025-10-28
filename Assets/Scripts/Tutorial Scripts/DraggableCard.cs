using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class DraggableCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Canvas canvas;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    private Vector3 originalPosition;
    private Vector3 originalScale;
    public float dragScale = 1.1f;
    public float returnDuration = 0.25f; // smooth return duration

    private Transform originalParent;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();
        originalPosition = rectTransform.position;
        originalScale = rectTransform.localScale;
        originalParent = transform.parent;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        StopAllCoroutines(); // stop any return animation

        // Full visibility
        canvasGroup.alpha = 1f; // no transparency
        canvasGroup.blocksRaycasts = false;

        // Scale up while dragging
        rectTransform.localScale = originalScale * dragScale;

        // Move card to top of canvas so it feels like "lifting"
        transform.SetParent(canvas.transform, true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Move the card smoothly with the pointer
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        // Smoothly return to original position and scale
        StartCoroutine(ReturnToOriginal());
    }

    private IEnumerator ReturnToOriginal()
    {
        Vector3 startPos = rectTransform.position;
        Vector3 startScale = rectTransform.localScale;
        float elapsed = 0f;

        while (elapsed < returnDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / returnDuration;
            rectTransform.position = Vector3.Lerp(startPos, originalPosition, t);
            rectTransform.localScale = Vector3.Lerp(startScale, originalScale, t);
            yield return null;
        }

        rectTransform.position = originalPosition;
        rectTransform.localScale = originalScale;

        // Return card to its original parent so it stays in proper UI hierarchy
        transform.SetParent(originalParent, true);
    }
}
