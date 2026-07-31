using UnityEngine;

public class HotbarInvetoryModel : MonoBehaviour
{
    [SerializeField] private int HotbarSlotsSize;
    private SlotData[] HotbarSlots;

    private void Awake()
    {
        Initialize(HotbarSlotsSize);
    }

    public void Initialize(int totalSlots)
    {
        HotbarSlots = new SlotData[totalSlots];
        for (int i = 0; i < totalSlots; i++)
        {
            HotbarSlots[i].Clear();
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
