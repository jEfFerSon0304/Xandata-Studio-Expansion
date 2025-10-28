using UnityEngine;
using UnityEngine.EventSystems;
using Unity.Netcode;

public class DropZone : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        var dragged = eventData.pointerDrag?.GetComponent<SkillCardUI>();
        if (dragged == null)
            return;

        // ✅ Only allow dropping if it's the player's turn
        if (GameState.Instance == null ||
            GameState.Instance.CurrentPlayerId != NetworkManager.Singleton.LocalClientId)
        {
            // Optional: give feedback to the player
            if (dragged.manager != null)
                dragged.manager.feedbackText.text = "You can’t use skills right now!";
            return;
        }

        // ✅ Allow skill use for the current turn player
        Debug.Log($"[DropZone] Skill dropped: {dragged.skillData.skillName}");
        dragged.manager.TryUseSkill(dragged.skillData);
    }
}
