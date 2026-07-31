using Lean.Pool;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private Transform HotbarUITransform;
    [SerializeField] private Transform InventoryUITransform;
    [SerializeField] private GameObject SlotPrefabs;
    [SerializeField] private GameObject UIPanel;

    [Header("Inventory Model")]
    [SerializeField] private IInventory inventoryModel;
    [SerializeField] private IInventory hotbarModel;

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
        UIPanel.SetActive(value);
    }
}
