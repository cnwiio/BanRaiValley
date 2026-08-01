using Lean.Pool;
using UnityEngine;

public class HotbarInvetoryModel : MonoBehaviour, IInventory
{
    [SerializeField] private int HotbarSlotsSize;
    /*[SerializeField]*/ private InventorySlotUI SlotUI;
    [SerializeField] private Transform SlotUI_Parent;
    [SerializeField] private GameObject SlotUI_Prefabs;

    private InventorySlotUI[] _SlotUI;
    private SlotData[] HotbarSlots;
    private SlotData SelectedSlots;
    private int selectedIndex = 0;
    public int SelectedIndex
    {
        get => selectedIndex;
        set
        {
            if (value < 0 || value >= HotbarSlots.Length || selectedIndex == value) return;
            selectedIndex = value;
            UpdateSelectedHotbarSlotUI(value);
        }
    }

    public int TotalSlot
    {
        get => HotbarSlotsSize;
        set => HotbarSlotsSize = value;
    }

    private void Awake()
    {
        Initialize(TotalSlot);
        UpdateSelectedHotbarSlotUI(SelectedIndex);
    }

    private void OnEnable()
    {
        EventBus<ChangeHotbarSlotEvent>.Subscribe(SetSelectSlot);
        EventBus<OnHotbarScrollActionEvent>.Subscribe(OnScrollAction);
    }
    private void OnDisable()
    {
        EventBus<ChangeHotbarSlotEvent>.Unsubscribe(SetSelectSlot);
        EventBus<OnHotbarScrollActionEvent>.Unsubscribe(OnScrollAction);
    }

    public void Initialize(int totalSlots)
    {
        HotbarSlots = new SlotData[totalSlots];
        for (int i = 0; i < totalSlots; i++)
        {
            HotbarSlots[i].Clear();
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

    #region Selected Slot
    public SlotData GetCurrentSelectSlotData()
    {
        return GetSlotData(SelectedIndex);
    }

    private void SetSelectSlot(ChangeHotbarSlotEvent evt)
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
        SelectedIndex = (SelectedIndex + 1) % HotbarSlots.Length;   
    }
    [ContextMenu("Previous Select Slot")]
    private void PrevSelectSlot()
    {
        SelectedIndex = (SelectedIndex - 1 + HotbarSlots.Length) % HotbarSlots.Length;   
    }

    private InventorySlotUI _oldHotslotUI;
    void UpdateSelectedHotbarSlotUI(int index)
    {
        _oldHotslotUI?.SetHighlight(false);
        _SlotUI[index].SetHighlight(true);
        _oldHotslotUI = _SlotUI[index];
    }
    #endregion

    private SlotData _tempSlot;
    public void SwapSlot(int indexA, int indexB)
    {
        if (!IsValidIndex(indexA) || !IsValidIndex(indexB) || indexA == indexB) return;

        _tempSlot = HotbarSlots[indexA];
        HotbarSlots[indexA] = HotbarSlots[indexB];
        HotbarSlots[indexB] = _tempSlot;
    }

    public void SwapSlotWithOther(SlotData data, int indexToSwap)
    {
        if (!IsValidIndex(indexToSwap)) return;

        HotbarSlots[indexToSwap] = data;
    }

    public SlotData GetSlotData(int index)
    {
        if (!IsValidIndex(index)) return default;
        return HotbarSlots[index];
    }

    private bool IsValidIndex(int index)
    {
        return index >= 0 && index < HotbarSlots.Length;
    }
}
