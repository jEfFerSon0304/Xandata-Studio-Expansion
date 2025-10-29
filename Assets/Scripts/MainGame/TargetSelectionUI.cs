using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using System.Collections.Generic;

public class TargetSelectionUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject targetButtonPrefab;   // prefab for each target entry (from Project folder)
    public Transform targetListParent;      // container for generated buttons
    public TMP_Text titleText;              // title text e.g. "Select Target"
    public GameObject panelRoot;            // main background panel (Image)
    public Button cancelButton; // 👈 NEW
    public TargetSelectionUI targetSelectionUI;
    public GameObject targetUIPanel;

    private MainGameManager manager;
    private CharacterDataSO.SkillData pendingSkill;


    private void Awake()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);

        if (cancelButton != null)
            cancelButton.onClick.AddListener(OnCancelClicked); // 👈 NEW
    }

    private void OnCancelClicked()
    {
        Debug.Log("[TargetUI] Player canceled target selection.");
        Close();
    }

    /// <summary>
    /// Opens the target selection panel and lists all valid players.
    /// </summary>
    public void Open(MainGameManager manager, CharacterDataSO.SkillData skill)
    {
        // Safety: prevent editor prefab deletion
        if (!Application.isPlaying)
        {
            Debug.LogWarning("[TargetUI] Tried to open while not playing. Ignored to prevent asset deletion.");
            return;
        }

        this.manager = manager;
        this.pendingSkill = skill;

        if (panelRoot == null)
        {
            Debug.LogError("[TargetUI] PanelRoot is not assigned!");
            return;
        }

        if (targetListParent == null)
        {
            Debug.LogError("[TargetUI] TargetListParent is not assigned!");
            return;
        }

        // Show the panel
        panelRoot.SetActive(true);
        titleText.text = $"Select Target for {skill.skillName}";

        // Clear old list safely
        foreach (Transform child in targetListParent)
        {
            if (Application.isPlaying)
                Destroy(child.gameObject);
            else
                DestroyImmediate(child.gameObject, false);
        }

        // Populate with players except yourself
        var gs = GameState.Instance;
        if (gs == null || gs.turnOrder == null || gs.turnOrder.Count == 0)
        {
            Debug.LogWarning("[TargetUI] No turn order available.");
            return;
        }

        foreach (var clientId in gs.turnOrder)
        {
            if (clientId == NetworkManager.Singleton.LocalClientId)
                continue;

            string targetName = GameDatabase.Instance.GetCharacterName(clientId);
            if (targetName == "Unknown")
            {
                Debug.LogWarning($"[TargetUI] Player {clientId} has unknown name. Showing placeholder instead.");
                targetName = $"Player {clientId}";
            }


            var btnObj = Instantiate(targetButtonPrefab, targetListParent);
            btnObj.GetComponentInChildren<TMP_Text>().text = targetName;


            ulong targetId = clientId;
            btnObj.GetComponent<Button>().onClick.AddListener(() =>
            {
                OnTargetChosen(targetId);
            });
        }

        Debug.Log($"[TargetUI] Turn order count: {GameState.Instance?.turnOrder.Count}");
        foreach (var cid in GameState.Instance.turnOrder)
        {
            Debug.Log($"[TargetUI] CID: {cid}, Name: {GameDatabase.Instance.GetCharacterName(cid)}");
        }


        Debug.Log($"[TargetUI] Opened for {skill.skillName}. Available targets: {targetListParent.childCount}");
    }

    private void OnTargetChosen(ulong targetClientId)
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);

        if (manager != null && pendingSkill != null)
        {
            manager.OnTargetSelected(pendingSkill, targetClientId);
        }
        else
        {
            Debug.LogWarning("[TargetUI] Missing manager or skill reference on target select.");
        }
    }

    public void Close()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);

        // ✅ Also hide the whole object
        gameObject.SetActive(false);
        targetSelectionUI.gameObject.SetActive(false);
        targetUIPanel.gameObject.SetActive(false);
    }

}
