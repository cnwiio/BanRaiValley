using Lean.Pool;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private Transform HotbarUITransform;
    [SerializeField] private CanvasGroup HotbarUICanvasGroup;
    [SerializeField] private Transform InventoryUITransform;
    [SerializeField] private CanvasGroup InventoryUICanvasGroup;
    [SerializeField] private GameObject SlotPrefabs;
    [SerializeField] private GameObject UIPanel;

    [Header("Inventory Model")]
    [SerializeField] private InventoyModel inventoryModel;
    [SerializeField] private HotbarInvetoryModel hotbarModel;

    private InventorySlotUI[] _inventorySlotUI, _hotSlotUI;

    private InventorySlotUI[] CreatSlotUI(int totalSlots, IInventory inventory, GameObject prefabs, Transform parentTransform)
    {
        InventorySlotUI[] slots = new InventorySlotUI[totalSlots];
        for (int i = 0; i < slots.Length; i++)
        {
            var go = LeanPool.Spawn(prefabs, parentTransform);
            slots[i] = go.GetComponent<InventorySlotUI>();
            slots[i].Setup(i, inventory);
        }

        return slots;
    }

    public void ToggleInventoryUI(bool value)
    {
        HotbarUICanvasGroup.alpha = 0;
        InventoryUICanvasGroup.alpha = 0;

        if (value)
        {
            _inventorySlotUI = CreatSlotUI(inventoryModel.TotalSlot, inventoryModel, SlotPrefabs, InventoryUITransform);
            _hotSlotUI = CreatSlotUI(hotbarModel.TotalSlot, hotbarModel, SlotPrefabs, HotbarUITransform);
        }
        else
        {
            for (int i = 0;i < _inventorySlotUI.Length; i++)
            {
                LeanPool.Despawn(_inventorySlotUI[i]);
            }

            for (int i = 0; i < _hotSlotUI.Length; i++)
            {
                LeanPool.Despawn(_hotSlotUI[i]);
            }
        }

        EventBus<InventoryUIRefreshEvent>.Raise(new InventoryUIRefreshEvent() { });
        HotbarUICanvasGroup.alpha = 1;
        InventoryUICanvasGroup.alpha = 1;
        UIPanel.SetActive(value);
    }
}
