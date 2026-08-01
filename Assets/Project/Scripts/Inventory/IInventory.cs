using Lean.Pool;
using System;
using UnityEngine;

public interface IInventory
{
    public int TotalSlot { get; set; }

    public SlotData GetSlotData(int index);
    public void SwapSlot(int indexA, int indexB);
    public void SetSlotData(SlotData data, int indexToSwap);
    public int AddStackItemToSlot(int slot, Item itemToAdd, int amount);
    public void ClearSlot(int index);
}
