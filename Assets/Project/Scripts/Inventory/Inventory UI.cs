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
    [SerializeField] private GameObject HotbarUIPanel;

    [Header("Inventory Model")]
    [SerializeField] private InventoyModel inventoryModel;
    [SerializeField] private HotbarInvetoryModel hotbarModel;

    private InventorySlotUI[] _inventorySlotUI, _hotSlotUI;
    private bool IsInventoryUIActive => UIPanel.activeSelf;

    private void OnEnable()
    {
        EventBus<InventoryToggleEvent>.Subscribe(ToggleInventoryUI);
    }
    private void OnDisable()
    {
        EventBus<InventoryToggleEvent>.Unsubscribe(ToggleInventoryUI);
    }

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

    public void ToggleInventoryUI(InventoryToggleEvent evt)
    {
        ToggleInventoryUI();
    }

    public void ToggleInventoryUI()
    {
        HotbarUICanvasGroup.alpha = 0;
        InventoryUICanvasGroup.alpha = 0;

        
        CreateAndDestroyUI(!IsInventoryUIActive);
        //SetCursorState(!IsInventoryUIActive);
        SetActionMapType(!IsInventoryUIActive);

        EventBus<InventoryUIRefreshEvent>.Raise(new InventoryUIRefreshEvent() { });

        HotbarUICanvasGroup.alpha = 1;
        InventoryUICanvasGroup.alpha = 1;
        UIPanel.SetActive(!IsInventoryUIActive);
        HotbarUIPanel.SetActive(!IsInventoryUIActive);
    }

    void CreateAndDestroyUI(bool value)
    {
        if (value)
        {
            _inventorySlotUI = CreatSlotUI(inventoryModel.TotalSlot, inventoryModel, SlotPrefabs, InventoryUITransform);
            _hotSlotUI = CreatSlotUI(hotbarModel.TotalSlot, hotbarModel, SlotPrefabs, HotbarUITransform);
        }
        else
        {
            for (int i = 0; i < _inventorySlotUI.Length; i++)
            {
                LeanPool.Despawn(_inventorySlotUI[i]);
            }

            for (int i = 0; i < _hotSlotUI.Length; i++)
            {
                LeanPool.Despawn(_hotSlotUI[i]);
            }
        }
    }

    void SetCursorState(bool isVisible)
    {
        Cursor.lockState = isVisible ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isVisible;
    }

    void SetActionMapType(bool value)
    {
        if (value)
        {
            EventBus<ChangeActionMap>.Raise(new ChangeActionMap() { MapType = ActionMapType.UI });
        }
        else
        {
            EventBus<ChangeActionMap>.Raise(new ChangeActionMap() { MapType = ActionMapType.Player });
        }
    }
}
