using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TurnOrderPopupUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Root GameObject of the popup panel.")]
    public GameObject panelRoot;

    [Tooltip("Background overlay button that closes the popup when clicked.")]
    public Button backgroundOverlay;

    [Tooltip("Parent Transform that holds all player entry objects.")]
    public Transform contentParent;

    [Tooltip("Prefab used to display each player in the turn order.")]
    public GameObject entryPrefab;

    [Header("Optional Controls")]
    [Tooltip("Button that toggles the popup visibility.")]
    public Button viewTurnOrderButton;

    private bool isVisible = false;
    private bool isReady = false;

    private void Awake()
    {
        // Make sure panel starts hidden
        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    private void OnEnable()
    {
        // ✅ Safer: start coroutine only when active
        if (!isReady)
            StartCoroutine(WaitForDependencies());

        // Hook up view button if assigned
        if (viewTurnOrderButton != null)
        {
            viewTurnOrderButton.onClick.RemoveAllListeners();
            viewTurnOrderButton.onClick.AddListener(TogglePopup);
        }

        // Hook up background overlay to close popup
        if (backgroundOverlay != null)
        {
            backgroundOverlay.onClick.RemoveAllListeners();
            backgroundOverlay.onClick.AddListener(HidePopup);
        }
    }

    private IEnumerator WaitForDependencies()
    {
        // Wait until GameState and GameDatabase exist
        while (GameState.Instance == null || GameDatabase.Instance == null)
            yield return null;

        // Wait until turn order is initialized
        while (GameState.Instance.GetCurrentTurnOrder() == null ||
               !GameState.Instance.GetCurrentTurnOrder().Any())
            yield return null;

        GameState.OnTurnOrderChangedEvent += RefreshList;
        isReady = true;

        Debug.Log("[TurnOrderPopupUI] Ready!");
        RefreshList();
    }

    public void TogglePopup()
    {
        if (isVisible)
            HidePopup();
        else
            ShowPopup();
    }

    public void ShowPopup()
    {
        if (!isReady || panelRoot == null) return;

        panelRoot.SetActive(true);
        isVisible = true;

        // Optional: animate in
        StartCoroutine(FadeInPanel());

        RefreshList();
    }

    public void HidePopup()
    {
        if (panelRoot == null) return;

        // Optional: animate out before hiding
        StartCoroutine(FadeOutPanel());
    }

    private void RefreshList()
    {
        if (!isReady || !isVisible) return;
        if (GameState.Instance == null || GameDatabase.Instance == null) return;

        // Clear previous entries
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        var order = GameState.Instance.GetCurrentTurnOrder()?.ToList();
        if (order == null || order.Count == 0)
        {
            Debug.LogWarning("[TurnOrderPopupUI] No players in turn order.");
            return;
        }

        int rank = 1;
        foreach (var clientId in order)
        {
            var entry = Instantiate(entryPrefab, contentParent);
            TMP_Text nameText = entry.GetComponentInChildren<TMP_Text>();

            string charName = GameDatabase.Instance.GetCharacterName(clientId);
            nameText.text = $"{rank}. {charName}";

            // Highlight current player
            if (GameState.Instance.CurrentPlayerId == clientId)
                nameText.color = Color.yellow;
            else
                nameText.color = Color.white;

            rank++;
        }
    }

    private IEnumerator FadeInPanel()
    {
        if (panelRoot == null) yield break;

        CanvasGroup group = panelRoot.GetComponent<CanvasGroup>();
        if (group == null)
            group = panelRoot.AddComponent<CanvasGroup>();

        group.alpha = 0;
        float t = 0;

        while (t < 1f)
        {
            t += Time.deltaTime * 3f;
            group.alpha = Mathf.Lerp(0, 1, t);
            yield return null;
        }

        group.alpha = 1;
    }

    private IEnumerator FadeOutPanel()
    {
        if (panelRoot == null) yield break;

        CanvasGroup group = panelRoot.GetComponent<CanvasGroup>();
        if (group == null)
            group = panelRoot.AddComponent<CanvasGroup>();

        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * 3f;
            group.alpha = Mathf.Lerp(1, 0, t);
            yield return null;
        }

        group.alpha = 0;
        panelRoot.SetActive(false);
        isVisible = false;
    }

    private void OnDestroy()
    {
        if (GameState.Instance != null)
            GameState.OnTurnOrderChangedEvent -= RefreshList;
    }
}
