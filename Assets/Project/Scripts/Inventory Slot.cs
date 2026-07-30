using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour, IDropHandler
{
    private GameObject droppedGo;
    private InventoryItem droppedInventoryItem;
    private InventoryItem childrenInventoryItem;

    [SerializeField] private Image image;
    [SerializeField] private Color SelectedColor;
    [SerializeField] private Color UnSelectedColor;

    public void Awake()
    {
        image.color = UnSelectedColor;
    }

    public void Select()
    {
        image.color = SelectedColor;
    }

    public void DeSelect()
    {
        image.color = UnSelectedColor;
    }
    public void OnDrop(PointerEventData eventData)
    {
        droppedGo = eventData.pointerDrag;
        droppedInventoryItem = droppedGo.GetComponent<InventoryItem>();
        if (transform.childCount > 0) 
        {
            if (droppedInventoryItem.item.stackable &&
                droppedInventoryItem.item == childrenInventoryItem.item &&
                childrenInventoryItem.count < childrenInventoryItem.item.MaxStack)
            {
                childrenInventoryItem.count += droppedInventoryItem.count;
                Destroy(droppedInventoryItem.gameObject);
            }
            return; 
        }
        droppedInventoryItem.parentAfterDrag = transform;
        childrenInventoryItem = droppedInventoryItem;
    }
}
