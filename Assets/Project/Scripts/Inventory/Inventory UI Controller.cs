using Lean.Pool;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InventoryUIController : MonoBehaviour
{
    [Header("Inventory References")]
    [SerializeField] private InventoyModel _mainInventory;
    [SerializeField] private HotbarInventoryModel _hotbarInventory;

    [Header("Drag UI")]
    [SerializeField] private Image _dragImage;
    [SerializeField] private TextMeshProUGUI _dragText;

    private Transform _dragTransform;
    private SlotData _heldSlotData;
    private IInventory _originInventory;
    private int _originSlotIndex = -1;
    private bool HasHeldItem => !_heldSlotData.IsEmpty;
    private Coroutine _cursorFollowCoroutine;

    private void OnEnable()
    {
        EventBus<OnUIBeginDragEvent>.Subscribe(OnBeginDrag);
        EventBus<OnUIDragEvent>.Subscribe(OnDrag);
        EventBus<OnUIEndDragEvent>.Subscribe(OnEndDrag);
        EventBus<OnUIDropEvent>.Subscribe(OnDrop);
        EventBus<InventoryToggleEvent>.Subscribe(OnToggleInventoryUI);
        EventBus<OnUISlotClickEvent>.Subscribe(OnSlotClicked);
        EventBus<OnUIBackgroundClickEvent>.Subscribe(OnBackgroundClicked);
        EventBus<InventoryValidateHeldItemEvent>.Subscribe(OnValidateHeldItem);
    }

    private void OnDisable()
    {
        EventBus<OnUIBeginDragEvent>.Unsubscribe(OnBeginDrag);
        EventBus<OnUIDragEvent>.Unsubscribe(OnDrag);
        EventBus<OnUIEndDragEvent>.Unsubscribe(OnEndDrag);
        EventBus<OnUIDropEvent>.Unsubscribe(OnDrop);
        EventBus<InventoryToggleEvent>.Unsubscribe(OnToggleInventoryUI);
        EventBus<OnUISlotClickEvent>.Unsubscribe(OnSlotClicked);
        EventBus<OnUIBackgroundClickEvent>.Unsubscribe(OnBackgroundClicked);
        EventBus<InventoryValidateHeldItemEvent>.Unsubscribe(OnValidateHeldItem);

        ReturnHeldItemToInventory();
    }

    private void Awake()
    {
        _dragTransform = _dragImage.transform;
    }

    private int _indexA, _indexB;
    private IInventory _inventoryA, _inventoryB;
    private InventorySlotUI _slotA, _slotB;

    private void OnSlotClicked(OnUISlotClickEvent evt)
    {
        if (evt.IsShiftPressed)
        {
            HandleShiftClick(evt);
        }
        else if (evt.IsCtrlPressed)
        {
            HandleCtrlClick(evt);
        }
        else if (evt.Button == PointerEventData.InputButton.Left)
        {
            HandleLeftClick(evt);
        }
        else if (evt.Button == PointerEventData.InputButton.Right)
        {
            HandleRightClick(evt);
        }

        EventBus<InventoryUIRefreshEvent>.Raise(new InventoryUIRefreshEvent());
    }

    private void HandleLeftClick(OnUISlotClickEvent evt)
    {
        if (!HasHeldItem)
        {
            SlotData taken = evt.Inventory.TakeStack(evt.Index);
            if (!taken.IsEmpty)
            {
                SetHeldItem(taken, evt.Inventory, evt.Index);
            }
        }
        else
        {
            SlotData slotData = evt.Inventory.GetSlotData(evt.Index);
            if (slotData.IsEmpty)
            {
                evt.Inventory.SetSlotData(_heldSlotData, evt.Index);
                ClearHeldItem();
            }
            else if (slotData.item == _heldSlotData.item && _heldSlotData.item.stackable)
            {
                int remaining = evt.Inventory.AddStackItemToSlot(evt.Index, _heldSlotData.item, _heldSlotData.count);
                if (remaining > 0)
                {
                    _heldSlotData.count = remaining;
                    UpdateDragIcon(_heldSlotData);
                }
                else
                {
                    ClearHeldItem();
                }
            }
            else
            {
                evt.Inventory.SetSlotData(_heldSlotData, evt.Index);
                SetHeldItem(slotData, evt.Inventory, evt.Index);
            }
        }
    }

    private void HandleRightClick(OnUISlotClickEvent evt)
    {
        if (!HasHeldItem)
        {
            SlotData takenHalf = evt.Inventory.TakeHalfStack(evt.Index);
            if (!takenHalf.IsEmpty)
            {
                SetHeldItem(takenHalf, evt.Inventory, evt.Index);
            }
        }
        else
        {
            bool added = evt.Inventory.AddSingleItemToSlot(evt.Index, _heldSlotData.item);
            if (added)
            {
                _heldSlotData.count--;
                if (_heldSlotData.count <= 0)
                {
                    ClearHeldItem();
                }
                else
                {
                    UpdateDragIcon(_heldSlotData);
                }
            }
        }
    }

    private void HandleCtrlClick(OnUISlotClickEvent evt)
    {
        if (!HasHeldItem)
        {
            SlotData singleTaken = evt.Inventory.TakeSingleItem(evt.Index);
            if (!singleTaken.IsEmpty)
            {
                SetHeldItem(singleTaken, evt.Inventory, evt.Index);
            }
        }
        else
        {
            SlotData slotData = evt.Inventory.GetSlotData(evt.Index);
            if (!slotData.IsEmpty && slotData.item == _heldSlotData.item && _heldSlotData.count < _heldSlotData.item.MaxStack)
            {
                SlotData singleTaken = evt.Inventory.TakeSingleItem(evt.Index);
                if (!singleTaken.IsEmpty)
                {
                    _heldSlotData.count += singleTaken.count;
                    UpdateDragIcon(_heldSlotData);
                }
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

        if (targetInventory.AutoStashItem(sourceData))
        {
            evt.Inventory.ClearSlot(evt.Index);
        }
    }

    private void OnBackgroundClicked(OnUIBackgroundClickEvent evt)
    {
        ReturnHeldItemToInventory();
    }

    private void OnValidateHeldItem(InventoryValidateHeldItemEvent evt)
    {
        ReturnHeldItemToInventory();
    }

    public void ReturnHeldItemToInventory()
    {
        if (!HasHeldItem) return;

        bool isStashed = _originInventory != null && _originInventory.AutoStashItem(_heldSlotData, _originSlotIndex);
        if (!isStashed && _mainInventory != null)
        {
            isStashed = _mainInventory.AutoStashItem(_heldSlotData);
        }
        if (!isStashed && _hotbarInventory != null)
        {
            _hotbarInventory.AutoStashItem(_heldSlotData);
        }

        ClearHeldItem();
        EventBus<InventoryUIRefreshEvent>.Raise(new InventoryUIRefreshEvent());
    }

    private void SetHeldItem(SlotData data, IInventory originInv, int originIndex)
    {
        _heldSlotData = data;
        _originInventory = originInv;
        _originSlotIndex = originIndex;
        if (Mouse.current != null)
        {
            _dragTransform.position = Mouse.current.position.ReadValue();
        }
        EnableDragIcon(_heldSlotData);

        if (_cursorFollowCoroutine == null)
        {
            _cursorFollowCoroutine = StartCoroutine(UpdateCursorFollowCoroutine());
        }
    }

    private void ClearHeldItem()
    {
        _heldSlotData.Clear();
        _originInventory = null;
        _originSlotIndex = -1;
        DisableDragIcon();

        if (_cursorFollowCoroutine != null)
        {
            StopCoroutine(_cursorFollowCoroutine);
            _cursorFollowCoroutine = null;
        }
    }

    private void UpdateDragIcon(SlotData slot)
    {
        EnableDragIcon(slot);
    }

    private IEnumerator UpdateCursorFollowCoroutine()
    {
        while (HasHeldItem)
        {
            if (Mouse.current != null)
            {
                _dragTransform.position = Mouse.current.position.ReadValue();
            }
            yield return null;
        }
    }

    void OnBeginDrag(OnUIBeginDragEvent evt)
    {
        if (HasHeldItem) return;

        _indexA = evt.Index;
        _inventoryA = evt.Inventory;
        _slotA = evt.SlotUI;
        var slotData = _inventoryA.GetSlotData(_indexA);
        if (slotData.IsEmpty) return;

        _slotA.ImageInvicible();
        EnableDragIcon(slotData);
    }

    void OnDrag(OnUIDragEvent evt)
    {
        if (HasHeldItem) return;
        _dragTransform.position = evt.Position;
    }

    void OnEndDrag(OnUIEndDragEvent evt)
    {
        if (HasHeldItem) return;

        if (_slotA != null) 
            _slotA.RenderVisual();

        DisableDragIcon();
    }

    void OnDrop(OnUIDropEvent evt)
    {
        if (HasHeldItem) return;

        if (_slotA == null) return;
        _indexB = evt.Index;
        _inventoryB = evt.Inventory;
        _slotB = evt.SlotUI;
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

            _slotA.RenderVisual();
            _slotB.RenderVisual();
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
            _slotA.RenderVisual();
            _slotB.RenderVisual();
            return;
        }
        else
        {
            _inventoryA.SetSlotData(itemB, _indexA);
            _inventoryB.SetSlotData(itemA, _indexB);

            _slotA.RenderVisual();
            _slotB.RenderVisual();
        }
    }

    private void EnableDragIcon(SlotData slot)
    {
        _dragImage.sprite = slot.item.image;
        _dragImage.enabled = true;
        if (slot.count > 1)
        {
            _dragText.SetText($"{slot.count}");
            _dragText.enabled = true;
        }
        else
        {
            _dragText.enabled = false;
        }
    }

    private void DisableDragIcon()
    {
        _dragImage.enabled = false;
        _dragText.enabled = false;
    }

    private void OnToggleInventoryUI(InventoryToggleEvent evt)
    {
        ReturnHeldItemToInventory();

        if (_slotA != null)
        {
            _slotA.RenderVisual();
            _slotA = null;
        }

        DisableDragIcon();
    }
}

