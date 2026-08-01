using System;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;


public class InventoyModel : BaseInventory
{
    private void Awake()
    {
        Initialize(TotalSlot);
    }

    public void Initialize(int totalSlots)
    {
        inventorySlots = new SlotData[totalSlots];
        for (int i = 0; i < totalSlots; i++)
        {
            inventorySlots[i].Clear();
        }
    }

    public void RemoveItem(int index, int amount)
    {
        if (!IsValidIndex(index) || inventorySlots[index].IsEmpty) return;

        inventorySlots[index].count -= amount;
        if (inventorySlots[index].count <= 0)
        {
            inventorySlots[index].Clear();
        }
    }
}
