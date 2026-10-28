using Lean.Pool;
using UnityEngine;

public class InventoryUI : MonoBehaviour, ICloseableUI
{
    [Header("UI Reference")]
    [SerializeField] private Transform HotbarUITransform;
    [SerializeField] private CanvasGroup HotbarUICanvasGroup;
    [SerializeField] private Transform InventoryUITransform;
    [SerializeField] private CanvasGroup InventoryUICanvasGroup;
    [Header("GameObject Reference")]
    [SerializeField] private GameObject SlotPrefabs;
    [SerializeField] private GameObject UIPanel;
    [SerializeField] private GameObject HotbarUIPanel;

    [Header("Inventory Model")]
    [SerializeField] private InventoyModel inventoryModel;
    [SerializeField] private HotbarInventoryModel hotbarModel;

    private InventorySlotUI[] _inventorySlotUI, _hotSlotUI;
    private bool IsInventoryUIActive => UIPanel != null && UIPanel.activeSelf;
    public bool IsOpen => IsInventoryUIActive;

    private void OnEnable()
    {
        EventBus<InventoryToggleEvent>.Subscribe(ToggleInventoryUI);
        EventBus<OnDayEndedEvent>.Subscribe(OnDayEnd);
    }
    private void OnDisable()
    {
        EventBus<InventoryToggleEvent>.Unsubscribe(ToggleInventoryUI);
        EventBus<OnDayEndedEvent>.Unsubscribe(OnDayEnd);
        UIStackManager.UnregisterClosed(this);
    }

    public void ToggleInventoryUI(InventoryToggleEvent evt)
    {
        ToggleInventoryUI();
    }
    
    private void OnDayEnd(OnDayEndedEvent evt)
    {
        if (IsInventoryUIActive)
            EventBus<InventoryToggleEvent>.Raise(new InventoryToggleEvent());
    }

    public void CloseUI()
    {
        if (IsInventoryUIActive)
        {
            EventBus<InventoryToggleEvent>.Raise(new InventoryToggleEvent());
        }
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
    
    public void ToggleInventoryUI()
    {
        HotbarUICanvasGroup.alpha = 0;
        InventoryUICanvasGroup.alpha = 0;

        bool willOpen = !IsInventoryUIActive;
        
        CreateAndDestroyUI(willOpen);

        EventBus<InventoryUIRefreshEvent>.Raise(new InventoryUIRefreshEvent() { });

        HotbarUICanvasGroup.alpha = 1;
        InventoryUICanvasGroup.alpha = 1;
        if (UIPanel != null)
            UIPanel.SetActive(willOpen);
        if (HotbarUIPanel != null)
            HotbarUIPanel.SetActive(!willOpen);

        if (willOpen)
        {
            UIStackManager.RegisterOpen(this);
        }
        else
        {
            UIStackManager.UnregisterClosed(this);
        }
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
            if (_inventorySlotUI != null)
            {
                for (int i = 0; i < _inventorySlotUI.Length; i++)
                {
                    LeanPool.Despawn(_inventorySlotUI[i]);
                }
            }

            if (_hotSlotUI != null)
            {
                for (int i = 0; i < _hotSlotUI.Length; i++)
                {
                    LeanPool.Despawn(_hotSlotUI[i]);
                }
            }
        }
    }
}
