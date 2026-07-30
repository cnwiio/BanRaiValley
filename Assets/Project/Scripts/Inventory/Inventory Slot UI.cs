using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    [Header("UI Component References")]
    [SerializeField] private Image boarderImage;
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text countText;

    [Header("Color")]
    [SerializeField] private Color SelectColor;
    [SerializeField] private Color UnSelectColor;

    private int SlotIndex;

    public void Setup(int index)
    {
        SlotIndex = index;
    }

    public void RenderVisual(Sprite icon, int count)
    {
        if (icon != null && count > 0)
        {
            iconImage.sprite = icon;
            iconImage.enabled = true;

            if (count > 1)
            {
                countText.text = count.ToString();
                countText.enabled = true;
            }
            else
            {
                countText.enabled = false;
            }
        }
        else
        {
            iconImage.enabled = false;
            countText.enabled = false;
        }
    }

    public void SetHighlight(bool isSelected)
    {
        if (boarderImage == null) return;

        boarderImage.color = isSelected ? SelectColor : UnSelectColor;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        EventBus<OnUIBeginDragEvent>.Raise(new OnUIBeginDragEvent() { Index = SlotIndex });
    }

    public void OnDrag(PointerEventData eventData)
    {
        EventBus<OnUIDragEvent>.Raise(new OnUIDragEvent() { Position = eventData.position });
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        EventBus<OnUIEndDragEvent>.Raise(new OnUIEndDragEvent() { });
    }

    public void OnDrop(PointerEventData eventData)
    {
        EventBus<OnUIDropEvent>.Raise(new OnUIDropEvent() { Index = SlotIndex });
    }

}
