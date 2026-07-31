using System;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;


public class InventoyModel : MonoBehaviour, IInventory
{
    [SerializeField] private int inventorySlotsSize;
    private SlotData[] inventorySlots;
    public int TotalSlot 
    {
        get => inventorySlotsSize;
        set => inventorySlotsSize = value;
    }

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

    /// <summary>
    /// Checks if the inventory has enough space to hold the specified amount of items without modifying inventory state.
    /// </summary>
    public bool CanAddItem(Item itemToAdd, int amount)
    {
        if (itemToAdd == null || amount <= 0) return false;

        int remainingNeeded = amount;
        bool isStackable = itemToAdd.stackable;
        int maxStack = isStackable ? itemToAdd.MaxStack : 1;

        // 1. Calculate space in existing matching stacks
        if (isStackable)
        {
            for (int i = 0; i < inventorySlots.Length; i++)
            {
                if (!inventorySlots[i].IsEmpty && inventorySlots[i].item == itemToAdd)
                {
                    int spaceInSlot = maxStack - inventorySlots[i].count;
                    if (spaceInSlot > 0)
                    {
                        remainingNeeded -= spaceInSlot;
                        if (remainingNeeded <= 0) return true;
                    }
                }
            }
        }

        // 2. Calculate space in empty slots
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (inventorySlots[i].IsEmpty)
            {
                remainingNeeded -= maxStack;
                if (remainingNeeded <= 0) return true;
            }
        }

        return remainingNeeded <= 0;
    }

    /// <summary>
    /// Directly adds items to inventory slots. Assumes space check has already been passed or fills as much as possible.
    /// </summary>
    public void AddItem(Item itemToAdd, int amount)
    {
        if (itemToAdd == null || amount <= 0) return;

        int remainingAmount = amount;
        bool isStackable = itemToAdd.stackable;
        int maxStack = isStackable ? itemToAdd.MaxStack : 1;

        // 1. Fill existing stackable slots first
        if (isStackable)
        {
            for (int i = 0; i < inventorySlots.Length; i++)
            {
                if (!inventorySlots[i].IsEmpty && inventorySlots[i].item == itemToAdd)
                {
                    int spaceInSlot = maxStack - inventorySlots[i].count;
                    if (spaceInSlot > 0)
                    {
                        int amountToAdd = Mathf.Min(remainingAmount ,spaceInSlot);
                        inventorySlots[i].count += amountToAdd;
                        remainingAmount -= amountToAdd;

                        if (remainingAmount <= 0) return;
                    }
                }
            }
        }

        // 2. Fill empty slots
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (inventorySlots[i].IsEmpty)
            {
                int amountToAdd = Mathf.Min(remainingAmount ,maxStack);
                inventorySlots[i].item = itemToAdd;
                inventorySlots[i].count = amountToAdd;
                remainingAmount -= amountToAdd;

                if (remainingAmount <= 0) return;
            }
        }
    }

    /// <summary>
    /// Safe wrapper method to check capacity first, then add items if possible.
    /// </summary>
    public bool TryAddItem(Item itemToAdd, int amount)
    {
        if (!CanAddItem(itemToAdd, amount)) return false;

        AddItem(itemToAdd, amount);
        EventBus<InventoryUIRefreshEvent>.Raise(new InventoryUIRefreshEvent() {});
        return true;
    }

    private SlotData _tempSlot;
    public void SwapSlot(int indexA, int indexB)
    {
        if (!IsValidIndex(indexA) || !IsValidIndex(indexB) || indexA == indexB) return;

        _tempSlot = inventorySlots[indexA];
        inventorySlots[indexA] = inventorySlots[indexB];
        inventorySlots[indexB] = _tempSlot;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"> Slot that want to swap with</param>
    /// <param name="indexToSwap"> Slot index of this inventory to swap with</param>
    public void SwapSlotWithOther(SlotData data, int indexToSwap)
    {
        if (!IsValidIndex(indexToSwap)) return;

        inventorySlots[indexToSwap] = data;
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

    public SlotData GetSlotData(int index)
    {
        if (!IsValidIndex(index)) return default;
        return inventorySlots[index];
    }

    private bool IsValidIndex(int index)
    {
        return index >= 0 && index < inventorySlots.Length;
    }
}
