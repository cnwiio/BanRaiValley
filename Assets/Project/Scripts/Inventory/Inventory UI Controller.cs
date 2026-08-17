using Lean.Pool;
using System;
using System.Collections;
using TMPro;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static UnityEngine.Rendering.VolumeComponent;

public class InventoryUIController : MonoBehaviour
{
    //[Header("Inventory Reference")]
    //[SerializeField] private InventoyModel inventoyModel;
    //[SerializeField] private GameObject inventorySlot_Prefabs;
    //[SerializeField] private Transform InventoryTransform;
    //[SerializeField] private GameObject InventoryUIPanel;
    //private InventorySlotUI[] inventorySlotUI;


    [Header("Drag UI")]
    [SerializeField] private Image DragImage;
    [SerializeField] private TextMeshProUGUI DragText;

    private Transform DragTransform;
    private Coroutine updateDragIconPosCoroutine;

    private void OnEnable()
    {
        EventBus<OnUIBeginDragEvent>.Subscribe(OnBeginDrag);
        EventBus<OnUIDragEvent>.Subscribe(OnDrag);
        EventBus<OnUIEndDragEvent>.Subscribe(OnEndDrag);
        EventBus<OnUIDropEvent>.Subscribe(OnDrop);
        EventBus<InventoryToggleEvent>.Subscribe(OnToggleInventoryUI);
    }
    private void OnDisable()
    {
        EventBus<OnUIBeginDragEvent>.Unsubscribe(OnBeginDrag);
        EventBus<OnUIDragEvent>.Unsubscribe(OnDrag);
        EventBus<OnUIEndDragEvent>.Unsubscribe(OnEndDrag);
        EventBus<OnUIDropEvent>.Unsubscribe(OnDrop);
        EventBus<InventoryToggleEvent>.Unsubscribe(OnToggleInventoryUI);
    }

    private void Awake()
    {
        DragTransform = DragImage.transform;
    }

    private int _indexA, _indexB;
    private IInventory _inventoryA, _inventoryB;
    private InventorySlotUI _slotA, _slotB;
    void OnBeginDrag(OnUIBeginDragEvent evt)
    {
        _indexA = evt.Index;
        _inventoryA = evt.Inventory;
        _slotA = evt.SlotUI;
        var slotData = _inventoryA.GetSlotData(_indexA);
        if (slotData.IsEmpty) return;

        //updateDragIconPosCoroutine = StartCoroutine(UpdateDragIconPosCoroutine());
        _slotA.ImageInvicible();
        EnableDragIcon(slotData);
    }

    void OnDrag(OnUIDragEvent evt)
    {
        DragTransform.position = evt.Position;
    }

    void OnEndDrag(OnUIEndDragEvent evt)
    {
        if (_slotA != null) 
            _slotA.RenderVisual();


        DisableDragIcon();
        //StopCoroutine(updateDragIconPosCoroutine);
    }

    void OnDrop(OnUIDropEvent evt)
    {
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

    private IEnumerator UpdateDragIconPosCoroutine()
    {
        while (true)
        {
            DragTransform.position = Mouse.current.position.ReadValue();
            yield return null;
        }
    }

    private void EnableDragIcon(SlotData slot)
    {
        DragImage.sprite = slot.item.image;
        DragImage.enabled = true;
        if (slot.count > 1)
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

    private void OnToggleInventoryUI(InventoryToggleEvent evt)
    {
        if (_slotA != null)
        {
            _slotA.RenderVisual();
            _slotA = null;
        }

        DisableDragIcon();
    }

    //private void OnDestroy()
    //{
    //    EventBus<InventoryToggleEvent>.Unsubscribe(OnToggleInventoryUI);
    //}
}
