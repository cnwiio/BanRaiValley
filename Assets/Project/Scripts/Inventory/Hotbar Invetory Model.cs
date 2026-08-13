using Lean.Pool;
using UnityEngine;

public class HotbarInventoryModel : BaseInventory
{
    [SerializeField] private Transform SlotUI_Parent;
    [SerializeField] private GameObject SlotUI_Prefabs;

    private InventorySlotUI[] _SlotUI;
    private int selectedIndex = 0;
    public int SelectedIndex
    {
        get => selectedIndex;
        set
        {
            if (value < 0 || value >= inventorySlots.Length || selectedIndex == value) return;
            selectedIndex = value;
            EventBus<OnHotbarChangeEvent>.Raise(new OnHotbarChangeEvent() { slotData = GetCurrentSelectSlotData() });
            UpdateSelectedHotbarSlotUI(value);
        }
    }

    private void Awake()
    {
        Initialize(TotalSlot);
        UpdateSelectedHotbarSlotUI(SelectedIndex);
    }

    private void OnEnable()
    {
        EventBus<OnHotbarSelectEvent>.Subscribe(SetSelectSlot);
        EventBus<OnHotbarScrollActionEvent>.Subscribe(OnScrollAction);
    }
    private void OnDisable()
    {
        EventBus<OnHotbarSelectEvent>.Unsubscribe(SetSelectSlot);
        EventBus<OnHotbarScrollActionEvent>.Unsubscribe(OnScrollAction);
    }

    public void Initialize(int totalSlots)
    {
        inventorySlots = new SlotData[totalSlots];
        for (int i = 0; i < totalSlots; i++)
        {
            inventorySlots[i].Clear();
        }

        CreateUI(this, totalSlots);
    }

    private void CreateUI(IInventory inventoy, int SlotsSize)
    {
        _SlotUI = new InventorySlotUI[SlotsSize];
        for (int i = 0; i < SlotsSize; i++)
        {
            //var go = Instantiate(inventorySlot_Prefabs, InventoryTransform);
            var go = LeanPool.Spawn(SlotUI_Prefabs, SlotUI_Parent);
            _SlotUI[i] = go.GetComponent<InventorySlotUI>();
            _SlotUI[i].Setup(i, inventoy);
        }
    }

    public override void SetSlotData(SlotData data, int indexToSwap)
    {
        base.SetSlotData(data, indexToSwap);
        if (indexToSwap == SelectedIndex)
        {
            EventBus<OnHotbarChangeEvent>.Raise(new OnHotbarChangeEvent() { slotData = GetCurrentSelectSlotData() });
        }
    }

    public override void SwapSlot(int indexA, int indexB)
    {
        base.SwapSlot(indexA, indexB);
        if (indexA == SelectedIndex || indexB == SelectedIndex)
        {
            EventBus<OnHotbarChangeEvent>.Raise(new OnHotbarChangeEvent() { slotData = GetCurrentSelectSlotData() });
        }
    }

    #region Selected Slot
    public SlotData GetCurrentSelectSlotData()
    {
        return GetSlotData(SelectedIndex);
    }

    private void SetSelectSlot(OnHotbarSelectEvent evt)
    {
        SetSelectSlot(evt.Index - 1);
    }
    private void SetSelectSlot(int index)
    {
        if (IsValidIndex(index))
            SelectedIndex = index;
    }

    private void OnScrollAction(OnHotbarScrollActionEvent evt)
    {
        if (evt.value > 0)
            NextSelectSlot();
        else if (evt.value < 0)
            PrevSelectSlot();
    }

    [ContextMenu("Next Select Slot")]
    private void NextSelectSlot()
    {
        SelectedIndex = (SelectedIndex + 1) % inventorySlots.Length;   
    }
    [ContextMenu("Previous Select Slot")]
    private void PrevSelectSlot()
    {
        SelectedIndex = (SelectedIndex - 1 + inventorySlots.Length) % inventorySlots.Length;   
    }

    private InventorySlotUI _oldHotslotUI;
    void UpdateSelectedHotbarSlotUI(int index)
    {
        _oldHotslotUI?.SetHighlight(false);
        _SlotUI[index].SetHighlight(true);
        _oldHotslotUI = _SlotUI[index];
    }
    #endregion
}
