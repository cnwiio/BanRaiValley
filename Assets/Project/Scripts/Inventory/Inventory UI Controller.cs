using Lean.Pool;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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

    private void OnEnable()
    {
        EventBus<OnUIBeginDragEvent>.Subscribe(OnBeginDrag);
        EventBus<OnUIDragEvent>.Subscribe(OnDrag);
        EventBus<OnUIEndDragEvent>.Subscribe(OnEndDrag);
        EventBus<OnUIDropEvent>.Subscribe(OnDrop);
    }
    private void OnDisable()
    {
        EventBus<OnUIBeginDragEvent>.Unsubscribe(OnBeginDrag);
        EventBus<OnUIDragEvent>.Unsubscribe(OnDrag);
        EventBus<OnUIEndDragEvent>.Unsubscribe(OnEndDrag);
        EventBus<OnUIDropEvent>.Unsubscribe(OnDrop);

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

        _slotA.ImageInvicible();
        EnableDragIcon(slotData);
    }

    void OnDrag(OnUIDragEvent evt)
    {
        DragTransform.position = evt.Position;
    }

    void OnEndDrag(OnUIEndDragEvent evt)
    {
        var slotData = _inventoryA.GetSlotData(_indexA);
        if (!slotData.IsEmpty)
            _slotA.RenderVisual();

        DisableDragIcon();
    }

    void OnDrop(OnUIDropEvent evt)
    {
        _indexB = evt.Index;
        _inventoryB = evt.Inventory;
        _slotB = evt.SlotUI;
        if (_inventoryA == _inventoryB)
        {
            _inventoryA.SwapSlot(_indexA, _indexB);
            var slotDataA = _inventoryA.GetSlotData(_indexA);
            var slotDataB = _inventoryA.GetSlotData(_indexB);
            if (!slotDataA.IsEmpty)
            {
                _slotA.RenderVisual();
            }
            if (!slotDataB.IsEmpty)
            {
                _slotB.RenderVisual();
            }
        }
        else
        {
            _inventoryA.SwapSlotWithOther(_inventoryB.GetSlotData(_indexB), _indexA);
            _inventoryB.SwapSlotWithOther(_inventoryA.GetSlotData(_indexA), _indexB);

            var slotDataA = _inventoryA.GetSlotData(_indexA);
            var slotDataB = _inventoryB.GetSlotData(_indexB);
            if (!slotDataA.IsEmpty)
            {
                _slotA.RenderVisual();
            }
            if (!slotDataB.IsEmpty)
            {
                _slotB.RenderVisual();
            }
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
}
