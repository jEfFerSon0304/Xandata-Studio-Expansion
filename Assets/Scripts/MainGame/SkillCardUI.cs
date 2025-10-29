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
    [HideInInspector] public int slotIndex; // assigned by MainGameManager or HandPanel


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
    // 🪄 Card Flip
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

        while (elapsed < halfDuration)
        {
            float angle = Mathf.Lerp(0, 90, elapsed / halfDuration);
            rectTransform.localEulerAngles = new Vector3(0, angle, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }

        isFlipped = !isFlipped;
        frontSide.SetActive(!isFlipped);
        backSide.SetActive(isFlipped);

        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            float angle = Mathf.Lerp(90, 180, elapsed / halfDuration);
            rectTransform.localEulerAngles = new Vector3(0, angle, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }

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
    // 🖱️ Drag Logic
    // ------------------------------------------------
    public void OnBeginDrag(PointerEventData eventData)
    {
        startPos = rectTransform.anchoredPosition;
        originalParent = transform.parent;

        // Store current slot position (so we can return exactly)
        slotIndex = transform.GetSiblingIndex();

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

        // Return to hand panel and original order
        transform.SetParent(originalParent, true);
        transform.SetSiblingIndex(slotIndex); // 👈 restores exact position order

        if (layoutGroup != null)
            layoutGroup.enabled = true;

        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.alpha = 1f;
        }

        StartCoroutine(SmoothReturn(startPos, 0.25f));
        isDragging = false;
    }


    // ------------------------------------------------
    // ✨ Smooth Return Animation
    // ------------------------------------------------
    private IEnumerator SmoothReturn(Vector2 targetPos, float duration)
    {
        Vector2 initialPos = rectTransform.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            t = 1f - Mathf.Pow(1f - t, 3f); // ease-out bounce
            rectTransform.anchoredPosition = Vector2.Lerp(initialPos, targetPos, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        rectTransform.anchoredPosition = targetPos;
    }

}
