using Lean.Pool;
using System;
using UnityEngine;

public interface IInventory
{
    public int TotalSlot { get; set; }

    public SlotData GetSlotData(int index);
    public void SwapSlot(int indexA, int indexB);
    public void SwapSlotWithOther(SlotData data, int indexToSwap);
}
