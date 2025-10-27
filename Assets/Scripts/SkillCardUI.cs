using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

public class SkillCardUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("UI References")]
    public TMP_Text nameText;
    public TMP_Text costText;
    public TMP_Text descText;
    public Image icon;

    [HideInInspector] public MainGameManager manager;
    [HideInInspector] public CharacterDataSO.SkillData skillData;

    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Canvas mainCanvas;
    private Vector2 startPos;
    private Transform originalParent;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
        mainCanvas = GetComponentInParent<Canvas>(); // 🩵 auto-assign canvas
    }

    public void Setup(CharacterDataSO.SkillData data)
    {
        skillData = data;

        nameText.text = data.skillName;
        costText.text = $"Cost: {data.energyCost}";
        descText.text = data.description;

        if (icon != null && data.skillIcon != null)
            icon.sprite = data.skillIcon;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (manager == null || GameState.Instance == null)
            return;

        // ✅ ensure drag only on player's turn
        if (GameState.Instance.CurrentPlayerId != NetworkManager.Singleton.LocalClientId)
            return;

        startPos = rectTransform.anchoredPosition;
        originalParent = transform.parent;

        transform.SetParent(manager.transform.root, true);
        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (rectTransform == null || mainCanvas == null)
            return; // 🧠 avoid nulls

        rectTransform.anchoredPosition += eventData.delta / mainCanvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (rectTransform == null)
            return;

        transform.SetParent(originalParent, true);
        rectTransform.anchoredPosition = startPos;

        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = true;
    }
}
