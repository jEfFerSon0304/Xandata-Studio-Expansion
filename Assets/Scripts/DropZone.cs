using UnityEngine;
using UnityEngine.EventSystems;

public class DropZone : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        var dragged = eventData.pointerDrag?.GetComponent<SkillCardUI>();
        if (dragged != null)
        {
            Debug.Log($"[DropZone] Skill dropped: {dragged.skillData.skillName}");
            dragged.manager.TryUseSkill(dragged.skillData);
        }
    }
}
