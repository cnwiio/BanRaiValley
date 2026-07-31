using Lean.Pool;
using UnityEngine;

public class HotbarInvetoryModel : MonoBehaviour, IInventory
{
    [SerializeField] private int HotbarSlotsSize;
    [SerializeField] private InventorySlotUI SlotUI;
    [SerializeField] private Transform SlotUI_Parent;
    [SerializeField] private GameObject SlotUI_Prefabs;
    private InventorySlotUI[] _SlotUI;
    private SlotData[] HotbarSlots;

    public int TotalSlot
    {
        get => HotbarSlotsSize;
        set => HotbarSlotsSize = value;
    }

    private void Awake()
    {
        Initialize(TotalSlot);
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
