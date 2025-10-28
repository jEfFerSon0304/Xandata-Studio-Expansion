using UnityEngine;
using UnityEngine.EventSystems;

public class DropZone : MonoBehaviour, IDropHandler
{
    public DialogueManager dialogueManager;

    public void OnDrop(PointerEventData eventData)
    {
        GameObject droppedCard = eventData.pointerDrag;
        if (droppedCard != null)
        {
            dialogueManager.OnCardDropped();
        }
    }
}
