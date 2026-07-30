using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryUIController : MonoBehaviour
{
    [Header("Inventory Reference")]
    [SerializeField] private InventoyModel inventoyModel;
    [SerializeField] private GameObject inventorySlot_Prefabs;
    [SerializeField] private Transform InventoryTransform;
    private InventorySlotUI[] inventorySlotUI;

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

    private void Start()
    {
        Initailize();
        RefreshUI();
    }

    void Initailize()
    {
        inventorySlotUI = new InventorySlotUI[inventoyModel.InventorySlotsSize];
        for (int i = 0; i < inventoyModel.InventorySlotsSize; i++)
        {
            var go = Instantiate(inventorySlot_Prefabs, InventoryTransform);
            inventorySlotUI[i] = go.GetComponent<InventorySlotUI>();
            inventorySlotUI[i].Setup(i);
        }
    }

    public void RefreshUI()
    {
        for (int i = 0; i < inventorySlotUI.Length; i++)
        {
            SlotData slotData = inventoyModel.GetSlotData(i);
            if (slotData.item != null)
            {
                inventorySlotUI[i].RenderVisual(slotData.item.image, slotData.count);
            } else
            {
                inventorySlotUI[i].RenderVisual(null, 0);
            }
        }
    }

    private int _indexA, _indexB;
    void OnBeginDrag(OnUIBeginDragEvent evt)
    {
        _indexA = evt.Index;
        var slot = inventoyModel.GetSlotData(_indexA);
        if (slot.item == null) return;

        inventorySlotUI[_indexA].RenderVisual(null, 0);
        DragImage.sprite = slot.item.image;
        DragImage.enabled = true;
        if (slot.count > 1)
        {
            DragText.SetText($"{slot.count}");
            DragText.enabled = true;
        }
    }

    void OnDrag(OnUIDragEvent evt)
    {
        DragTransform.position = evt.Position;
    }

    void OnEndDrag(OnUIEndDragEvent evt)
    {
        //RefreshUI();
        var slot = inventoyModel.GetSlotData(_indexA);
        if (slot.item != null)
            inventorySlotUI[_indexA].RenderVisual(slot.item.image, slot.count);

        DragImage.enabled = false;
        DragText.enabled = false;
    }

    void OnDrop(OnUIDropEvent evt)
    {
        _indexB = evt.Index;
        inventoyModel.SwapSlot(_indexA, _indexB);
        RefreshUI();
    }

}
