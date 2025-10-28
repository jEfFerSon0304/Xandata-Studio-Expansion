using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using System.Collections;

public class SkillCardUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    [Header("UI References")]
    public TMP_Text nameText;
    public TMP_Text costText;
    public TMP_Text descText;
    public Image icon;

    [Header("Card Sides")]
    public GameObject frontSide;
    public GameObject backSide;

    [HideInInspector] public MainGameManager manager;
    [HideInInspector] public CharacterDataSO.SkillData skillData;

    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Canvas mainCanvas;
    private Vector2 startPos;
    private Transform originalParent;
    private bool isFlipped = false;
    private bool isDragging = false;

    private HorizontalOrVerticalLayoutGroup layoutGroup;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
        mainCanvas = GetComponentInParent<Canvas>();
        layoutGroup = GetComponentInParent<HorizontalOrVerticalLayoutGroup>();
    }

    void Start()
    {
        ShowFront();
    }

    public void Setup(CharacterDataSO.SkillData data)
    {
        skillData = data;

        nameText.text = data.skillName;
        costText.text = $"Cost: {data.energyCost}";
        descText.text = data.description;

        if (icon != null && data.skillIcon != null)
            icon.sprite = data.skillIcon;

        ShowFront();
    }

    // ------------------------------------------------
    // 🪄 Realistic Flip Animation
    // ------------------------------------------------
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isDragging)
            StartCoroutine(FlipCardAnimation());
    }

    IEnumerator FlipCardAnimation()
    {
        float duration = 0.3f;
        float halfDuration = duration / 2f;
        float elapsed = 0f;

        Vector3 startRotation = rectTransform.localEulerAngles;
        Vector3 endRotation = startRotation + new Vector3(0, 180f, 0);

        // 🌀 First half: rotate to edge
        while (elapsed < halfDuration)
        {
            float angle = Mathf.Lerp(0, 90, elapsed / halfDuration);
            rectTransform.localEulerAngles = new Vector3(0, angle, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 🔁 Switch sides at the halfway point
        isFlipped = !isFlipped;
        frontSide.SetActive(!isFlipped);
        backSide.SetActive(isFlipped);

        // 🌀 Second half: complete rotation
        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            float angle = Mathf.Lerp(90, 180, elapsed / halfDuration);
            rectTransform.localEulerAngles = new Vector3(0, angle, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // ✅ Reset rotation (avoid cumulative rotation)
        rectTransform.localEulerAngles = Vector3.zero;
    }

    private void ShowFront()
    {
        frontSide.SetActive(true);
        backSide.SetActive(false);
        isFlipped = false;
        rectTransform.localEulerAngles = Vector3.zero;
    }

    // ------------------------------------------------
    // 🖱️ Drag Logic (unchanged, but stable)
    // ------------------------------------------------
    public void OnBeginDrag(PointerEventData eventData)
    {
        startPos = rectTransform.anchoredPosition;
        originalParent = transform.parent;

        if (layoutGroup != null)
            layoutGroup.enabled = false;

        transform.SetParent(mainCanvas.transform, true);

        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = 0.8f;
        }

        isDragging = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging || rectTransform == null || mainCanvas == null)
            return;

        rectTransform.anchoredPosition += eventData.delta / mainCanvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging)
            return;

        transform.SetParent(originalParent, true);
        rectTransform.anchoredPosition = startPos;

        if (layoutGroup != null)
            layoutGroup.enabled = true;

        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.alpha = 1f;
        }

        isDragging = false;
    }
}
