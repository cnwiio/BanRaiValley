using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InventoryUIController : MonoBehaviour
{
    //[Header("Inventory Reference")]
    //[SerializeField] private InventoyModel inventoyModel;
    //[SerializeField] private GameObject inventorySlot_Prefabs;
    //[SerializeField] private Transform InventoryTransform;
    //[SerializeField] private GameObject InventoryUIPanel;
    //private InventorySlotUI[] inventorySlotUI;

    [SerializeField] private InventoyModel _mainInventory;
    [SerializeField] private HotbarInventoryModel _hotbarInventory;


    [Header("Drag UI")]
    [SerializeField] private Image DragImage;
    [SerializeField] private TextMeshProUGUI DragText;


    private Transform DragTransform;
    private Coroutine updateDragIconPosCoroutine;

    private bool HasHeldItem => !_heldSlotData.IsEmpty;
    
    #region Binding
    private void OnEnable()
    {
        EventBus<OnUISlotClickEvent>.Subscribe(OnClick);
        EventBus<OnUIBeginDragEvent>.Subscribe(OnBeginDrag);
        EventBus<OnUIDragEvent>.Subscribe(OnDrag);
        EventBus<OnUIEndDragEvent>.Subscribe(OnEndDrag);
        EventBus<OnUIDropEvent>.Subscribe(OnDrop);
        EventBus<InventoryToggleEvent>.Subscribe(OnToggleInventoryUI);
    }
    private void OnDisable()
    {
        EventBus<OnUISlotClickEvent>.Unsubscribe(OnClick);
        EventBus<OnUIBeginDragEvent>.Unsubscribe(OnBeginDrag);
        EventBus<OnUIDragEvent>.Unsubscribe(OnDrag);
        EventBus<OnUIEndDragEvent>.Unsubscribe(OnEndDrag);
        EventBus<OnUIDropEvent>.Unsubscribe(OnDrop);
        EventBus<InventoryToggleEvent>.Unsubscribe(OnToggleInventoryUI);
    }
    private void Awake()
    {
        DragTransform = DragImage.transform;
        EventSystem.current.pixelDragThreshold = 60;
    }
    #endregion

    private int _indexA, _indexB;
    private IInventory _inventoryA, _inventoryB;
    private InventorySlotUI _slotUI_A, _slotUI_B;
    void OnBeginDrag(OnUIBeginDragEvent evt)
    {
        if (HasHeldItem) return;
        
        _indexA = evt.Index;
        _inventoryA = evt.Inventory;
        _slotUI_A = evt.SlotUI;
        var slotData = _inventoryA.GetSlotData(_indexA);
        if (slotData.IsEmpty) return;

        //updateDragIconPosCoroutine = StartCoroutine(UpdateDragIconPosCoroutine());
        _slotUI_A.ImageInvicible();
        EnableDragIcon(slotData);
    }
    
    void OnDrag(OnUIDragEvent evt)
    {
        if (HasHeldItem) return;
        
        DragTransform.position = evt.Position;
    }

    void OnEndDrag(OnUIEndDragEvent evt)
    {
        if (HasHeldItem) return;
        
        if (_slotUI_A != null) 
            _slotUI_A.RenderVisual();


        DisableDragIcon();
        //StopCoroutine(updateDragIconPosCoroutine);
    }

    void OnDrop(OnUIDropEvent evt)
    {
        if (HasHeldItem) return;
        
        if (_slotUI_A == null) return;
        _indexB = evt.Index;
        _inventoryB = evt.Inventory;
        _slotUI_B = evt.SlotUI;
        SlotData itemA = _inventoryA.GetSlotData(_indexA);
        SlotData itemB = _inventoryB.GetSlotData(_indexB); 
        if (itemA.IsEmpty) return;

        if (_inventoryA == _inventoryB)
        {
            if (_indexA == _indexB) return;

            if (itemA.item == itemB.item && itemA.item.stackable)
            {
                int remainingAmount = _inventoryB.AddStackItemToSlot(_indexB, itemA.item, itemA.count);
                if (remainingAmount > 0)
                {
                    itemB.count = remainingAmount;
                    _inventoryA.SetSlotData(itemB, _indexA);
                }
                else
                {
                    _inventoryA.ClearSlot(_indexA);
                }
            }
            else
            {
                _inventoryA.SwapSlot(_indexA, _indexB);
            }

            _slotUI_A.RenderVisual();
            _slotUI_B.RenderVisual();
        }
        else if (itemA.item == itemB.item && itemA.item.stackable)
        {
            int remainingAmount = _inventoryB.AddStackItemToSlot(_indexB, itemA.item, itemA.count);
            if (remainingAmount > 0)
            {
                itemB.count = remainingAmount;
                _inventoryA.SetSlotData(itemB, _indexA);
            }
            else
            {
                _inventoryA.ClearSlot(_indexA);
            }
            _slotUI_A.RenderVisual();
            _slotUI_B.RenderVisual();
            return;
        }
        else
        {
            
            _inventoryA.SetSlotData(itemB, _indexA);
            _inventoryB.SetSlotData(itemA, _indexB);

            _slotUI_A.RenderVisual();
            _slotUI_B.RenderVisual();
        }
    }
    
    void OnClick(OnUISlotClickEvent evt)
    {
        if (evt.IsShiftPressed)
        {
            HandleShiftClick(evt);
        }
        else if (evt.Button == PointerEventData.InputButton.Left)
        {
            HandleLeftClick(evt);
        }
        else if (evt.Button == PointerEventData.InputButton.Right)
        {
            HandleRightClick(evt);   
        }
        
        evt.SlotUI.RenderVisual();
    }
    
    void HandleLeftClick(OnUISlotClickEvent evt)
    {
        if (!HasHeldItem) // not holding a item
        { // pick up a item
            SlotData taken = evt.Inventory.TakeStack(evt.Index);
            if (!taken.IsEmpty)
            {
                SetHeldItem(taken, evt.Inventory, evt.Index);
            }
        }
        else // holding a item
        { // place a item, swap holding item if not empty
            var slotData = evt.Inventory.GetSlotData(evt.Index);
            if (slotData.IsEmpty) // empty slot
            {
                evt.Inventory.SetSlotData(_heldSlotData, evt.Index);
                ClearHeldItem();
            }
            else if (!slotData.IsEmpty && slotData.item.stackable && _heldSlotData.item == slotData.item) //stackable slot
            {
                var remainingAmount = evt.Inventory.AddStackItemToSlot(evt.Index, _heldSlotData.item, _heldSlotData.count);
                if (remainingAmount > 0)
                {
                    _heldSlotData.count = remainingAmount;
                    UpdateDragIcon(_heldSlotData);
                }
                else
                {
                    ClearHeldItem();
                }
            }
            else //non stackable slot
            {
                evt.Inventory.SetSlotData(_heldSlotData, evt.Index);
                SetHeldItem(slotData, evt.Inventory, evt.Index);   
            }
        }
    }

    void HandleRightClick(OnUISlotClickEvent evt)
    {
        if (!HasHeldItem) // not holding a item
        { 
            SlotData taken = evt.Inventory.TakeHalfStack(evt.Index);
            if (!taken.IsEmpty)
            {
                SetHeldItem(taken, evt.Inventory, evt.Index);
            }
        }
        else // holding a item
        {
            var slotData = evt.Inventory.GetSlotData(evt.Index);
            int halfCount = _heldSlotData.count / 2;
            
            if (slotData.IsEmpty) // empty slot
            {
                _heldSlotData.count -= halfCount;
                var remainingAmount = evt.Inventory.AddItemToEmptySlot(evt.Index, _heldSlotData.item, halfCount);
                _heldSlotData.count += remainingAmount;
            }
            else if (!slotData.IsEmpty && slotData.item.stackable && _heldSlotData.item == slotData.item) //stackable slot
            {
                _heldSlotData.count -= halfCount;
                var remainingAmount = evt.Inventory.AddStackItemToSlot(evt.Index, _heldSlotData.item, halfCount);
                _heldSlotData.count += remainingAmount;
            }
            else // non stackable slot
            {
                evt.Inventory.SetSlotData(_heldSlotData, evt.Index);
                SetHeldItem(slotData, evt.Inventory, evt.Index);   
            }
            
            if (_heldSlotData.IsEmpty)
            {
                ClearHeldItem();
            }
            else
            {
                UpdateDragIcon(_heldSlotData);
            }
        }
    }

    private void HandleShiftClick(OnUISlotClickEvent evt)
    {
        IInventory targetInventory = null;
        if (evt.Inventory == (IInventory)_mainInventory)
        {
            targetInventory = _hotbarInventory;
        }
        else if (evt.Inventory == (IInventory)_hotbarInventory)
        {
            targetInventory = _mainInventory;
        }

        if (targetInventory == null) return;

        SlotData sourceData = evt.Inventory.GetSlotData(evt.Index);
        if (sourceData.IsEmpty) return;

        int remaining = 0;
        if (sourceData.item.stackable)
        {
            remaining = targetInventory.StackExistingGetLeftover(sourceData.item, sourceData.count);
            sourceData.count = remaining;
        }

        if (sourceData.count > 0)
        {
            remaining = targetInventory.AddItemGetLeftover(sourceData.item, sourceData.count);
            sourceData.count = remaining;
        }

        if (remaining == 0)
        {
            evt.Inventory.ClearSlot(evt.Index);
        }
    }

    private IEnumerator UpdateDragIconPosCoroutine()
    {
        while (HasHeldItem)
        {
            DragTransform.position = Mouse.current.position.ReadValue();
            yield return null;
        }

        updateDragIconPosCoroutine = null;
    }

    private SlotData _heldSlotData;
    private IInventory _heldInventory;
    private int _heldIndex;
    private void SetHeldItem(SlotData slotData, IInventory inventory, int index)
    {
        _heldSlotData = slotData;
        _heldInventory = inventory;
        _heldIndex = index;
        EnableDragIcon(slotData);
        updateDragIconPosCoroutine ??= StartCoroutine(UpdateDragIconPosCoroutine());
    }

    private void ClearHeldItem()
    {
        _heldSlotData.Clear();
        _heldInventory = null;
        _heldIndex = -1;
        DisableDragIcon();
        if (updateDragIconPosCoroutine != null)
        {
            StopCoroutine(updateDragIconPosCoroutine);
            updateDragIconPosCoroutine = null;
        }
    }
    
    private void EnableDragIcon(SlotData slot)
    {
        DragImage.sprite = slot.item.image;
        DragImage.enabled = true;
        DragText.enabled = false;
        if (slot.item.stackable)
        {
            DragText.SetText($"{slot.count}");
            DragText.enabled = true;
        }
    }

    private void DisableDragIcon()
    {
        DragImage.enabled = false;
        DragText.enabled = false;
    }

    private void UpdateDragIcon(SlotData slot)
    {
        EnableDragIcon(slot);
    }
    
    private void OnToggleInventoryUI(InventoryToggleEvent evt)
    {
        if (_slotUI_A != null)
        {
            _slotUI_A.RenderVisual();
            _slotUI_A = null;
        }

        DisableDragIcon();
    }

    //private void OnDestroy()
    //{
    //    EventBus<InventoryToggleEvent>.Unsubscribe(OnToggleInventoryUI);
    //}
}
