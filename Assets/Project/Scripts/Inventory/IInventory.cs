using UnityEngine;

public interface IInventory
{
    public SlotData GetSlotData(int index);
    public void SwapSlot(int indexA, int indexB);
}
