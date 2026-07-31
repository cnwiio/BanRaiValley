using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;


public class InventoyModel : MonoBehaviour, IInventory
{
    [SerializeField] private int inventorySlotsSize;
    private SlotData[] inventorySlots;
    public int InventorySlotsSize => inventorySlotsSize;

    private void Awake()
    {
        Initialize(inventorySlotsSize);
    }

    public void Initialize(int totalSlots)
    {
        inventorySlots = new SlotData[totalSlots];
        for (int i = 0; i < totalSlots; i++)
        {
            inventorySlots[i].Clear();
        }
    }

    public bool TryAddItem(Item itemToAdd, int amount)
    {
        if (itemToAdd == null || amount <= 0) return false;
        int remainingAmount = amount;

        // check possible slot to add stack
        if (itemToAdd.stackable)
        {
            for (int i = 0; i < inventorySlots.Length; i++)
            {
                if (inventorySlots[i].item == itemToAdd && !inventorySlots[i].IsEmpty)
                {
                    int spaceInSlot = inventorySlots[i].item.MaxStack - inventorySlots[i].count;
                    if (spaceInSlot > 0)
                    {
                        int amountToAdded = Mathf.Min(spaceInSlot, remainingAmount);
                        inventorySlots[i].count += amountToAdded;
                        remainingAmount -= amountToAdded;

                        if (remainingAmount <= 0) return true;
                    }
                }
            }
        }

        // check first empty slot 
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (inventorySlots[i].IsEmpty)
            {
                int maxAllowed = itemToAdd.stackable ? itemToAdd.MaxStack : 1;
                int amountToAdd = Mathf.Min(remainingAmount, maxAllowed);

                inventorySlots[i].item = itemToAdd;
                inventorySlots[i].count = amountToAdd;
                remainingAmount -= amountToAdd;

                if (remainingAmount <= 0) return true;
            }
        }

        return remainingAmount == 0;
    }

    private SlotData _tempSlot;
    public void SwapSlot(int indexA, int indexB)
    {
        if (!IsValidIndex(indexA) || !IsValidIndex(indexB) || indexA == indexB) return;

        _tempSlot = inventorySlots[indexA];
        inventorySlots[indexA] = inventorySlots[indexB];
        inventorySlots[indexB] = _tempSlot;
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
