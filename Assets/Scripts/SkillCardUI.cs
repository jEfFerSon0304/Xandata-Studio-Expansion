using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

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

    // ----------------------------
    // 🪄 Flip Logic
    // ----------------------------
    public void OnPointerClick(PointerEventData eventData)
    {
        FlipCard();
    }

    private void FlipCard()
    {
        isFlipped = !isFlipped;
        frontSide.SetActive(!isFlipped);
        backSide.SetActive(isFlipped);
    }

    private void ShowFront()
    {
        frontSide.SetActive(true);
        backSide.SetActive(false);
        isFlipped = false;
    }

    // ----------------------------
    // 🖱️ Drag Logic
    // ----------------------------
    public void OnBeginDrag(PointerEventData eventData)
    {
        startPos = rectTransform.anchoredPosition;
        originalParent = transform.parent;

        // 🧩 Disable layout group so cards don't shift
        if (layoutGroup != null)
            layoutGroup.enabled = false;

        // 🧩 Move card to top-level canvas so it stays visible
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

        // ✅ Re-enable layout group
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
