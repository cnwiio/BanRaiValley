using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryBackgroundClickDetector : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        EventBus<OnUIBackgroundClickEvent>.Raise(new OnUIBackgroundClickEvent());
    }
}
