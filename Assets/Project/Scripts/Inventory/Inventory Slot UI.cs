using Lean.Pool;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour, IPoolable, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerClickHandler
{
    [Header("Model Reference")]
    [SerializeField] private IInventory inventoryModel;

    [Header("UI Component References")]
    [SerializeField] private Image boarderImage;
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text countText;

    [Header("Color")]
    [SerializeField] private Color SelectColor;
    [SerializeField] private Color UnSelectColor;

    private int SlotIndex;

    public void Setup(int index, IInventory inventory)
    {
        SlotIndex = index;
        inventoryModel = inventory;
    }

    public void RenderVisual(InventoryUIRefreshEvent evt)
    {
        RenderVisual();
    }

    public void RenderVisual()
    {
        var SlotData = inventoryModel.GetSlotData(SlotIndex);
        if (!SlotData.IsEmpty)
        {
            var icon = SlotData.item.image;
            var count = SlotData.count;

            iconImage.sprite = icon;
            iconImage.enabled = true;

            if (SlotData.item.stackable && count > 1)
            {
                countText.SetText($"{count}");
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

    public void ImageInvicible()
    {
        iconImage.enabled = false;
        countText.enabled = false;
    }

    public void SetHighlight(bool isSelected)
    {
        if (boarderImage == null) return;

        boarderImage.color = isSelected ? SelectColor : UnSelectColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.dragging) return;

        bool isShift = Keyboard.current != null && (Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed);
        bool isCtrl = Keyboard.current != null && (Keyboard.current.leftCtrlKey.isPressed || Keyboard.current.rightCtrlKey.isPressed);

        EventBus<OnUISlotClickEvent>.Raise(new OnUISlotClickEvent
        {
            Index = SlotIndex,
            Inventory = inventoryModel,
            SlotUI = this,
            Button = eventData.button,
            IsShiftPressed = isShift,
            IsCtrlPressed = isCtrl
        });
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        EventBus<OnUIBeginDragEvent>.Raise(new OnUIBeginDragEvent() { Index = SlotIndex , Inventory = inventoryModel, SlotUI = this});
    }

    public void OnDrag(PointerEventData eventData)
    {
        EventBus<OnUIDragEvent>.Raise(new OnUIDragEvent() { Position = eventData.position });
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        EventBus<OnUIEndDragEvent>.Raise(new OnUIEndDragEvent() { Inventory = inventoryModel , SlotUI = this });
    }

    public void OnDrop(PointerEventData eventData)
    {
        EventBus<OnUIDropEvent>.Raise(new OnUIDropEvent() { Index = SlotIndex, Inventory = inventoryModel , SlotUI = this });
    }

    public void OnSpawn()
    {
        EventBus<InventoryUIRefreshEvent>.Subscribe(RenderVisual);
    }

    public void OnDespawn()
    {
        EventBus<InventoryUIRefreshEvent>.Unsubscribe(RenderVisual);
    }

    private void OnDestroy()
    {
        EventBus<InventoryUIRefreshEvent>.Unsubscribe(RenderVisual);
    }
}
