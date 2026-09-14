using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using IPoolable = Lean.Pool.IPoolable;

public class InventorySlotUI : MonoBehaviour, IPoolable, 
    IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, 
    IPointerClickHandler, /*IPointerDownHandler, */
    IPointerEnterHandler, IPointerExitHandler
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
    
    [Header("Drag Settings")]
    [SerializeField] private float dragHoldDelay = 0.1f;
    
    private int SlotIndex;
    // private float _pointerDownTime;

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

            if (SlotData.item.type == ItemType.Seed ||
                SlotData.item.type == ItemType.Plant)
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
        
        bool isShiftPressed = Keyboard.current != null && (Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed);
        bool isCtrlPressed = Keyboard.current != null && (Keyboard.current.leftCtrlKey.isPressed || Keyboard.current.rightCtrlKey.isPressed);
        
        EventBus<OnUISlotClickEvent>.Raise(new OnUISlotClickEvent()
        {
            Index = SlotIndex,
            Inventory = inventoryModel,
            SlotUI = this,
            Button = eventData.button,
            IsShiftPressed = isShiftPressed,
            IsCtrlPressed = isCtrlPressed
        });
    }

    // public void OnPointerDown(PointerEventData eventData)
    // {
    //     _pointerDownTime = Time.unscaledTime;
    // }
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        // if (Time.unscaledTime - _pointerDownTime < dragHoldDelay)
        // {
        //     eventData.pointerDrag = null;
        //     OnPointerClick(eventData);
        //     return;
        // }
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

    public void OnPointerEnter(PointerEventData eventData)
    {
        SetHighlight(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetHighlight(false);
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
