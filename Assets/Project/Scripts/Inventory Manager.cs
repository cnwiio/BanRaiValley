using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public InventorySlot[] inventorySlots;
    public InventorySlot[] HotbarSlots;
    public InventorySlot[] InventoryHotbarSlots;
    public GameObject inventoryPrefab;

    int selectSlot = -1;

    private void Start()
    {
        ChangeSelectSlot(0); 
    }

    void ChangeSelectSlot(int value)
    {
        if (selectSlot >= 0)
            HotbarSlots[selectSlot].DeSelect();

        HotbarSlots[value].Select();
        selectSlot = value;
    }

    public bool AddItem(Item item)
    {
        // Check if any slot has the same item with count lower than max
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            InventorySlot slot = inventorySlots[i];
            InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
            if (itemInSlot != null &&
                itemInSlot.item == item &&
                itemInSlot.count < itemInSlot.item.MaxStack &&
                itemInSlot.item.stackable == true)
            {
                itemInSlot.count++;
                return true;
            }
        }

        // Find empty slot
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            InventorySlot slot = inventorySlots[i];
            InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
            if (itemInSlot == null)
            {
                SpawnNewItem(item, slot);
                return true;
            }
        }
        return false;
    }

    void SpawnNewItem(Item item, InventorySlot slot)
    {
        GameObject newItemGo = Instantiate(inventoryPrefab, slot.transform); 
        InventoryItem inventoryItem = newItemGo.GetComponent<InventoryItem>();
        inventoryItem.InitialiseItem(item);
    }

    public void SyncInventoryToHotbarSlot()
    {
        for (int i = 0; i < InventoryHotbarSlots.Length; i++)
        {
            InventorySlot slot = InventoryHotbarSlots[i];
            InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
            if (itemInSlot != null)
            {
                itemInSlot.transform.SetParent(HotbarSlots[i].transform);
            }
        }
    }
    public void SyncHotbarToInventorySlot()
    {
        for (int i = 0; i < HotbarSlots.Length; i++)
        {
            InventorySlot slot = HotbarSlots[i];
            InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
            if (itemInSlot != null)
            {
                itemInSlot.transform.SetParent(InventoryHotbarSlots[i].transform);
            }
        }
    }
}
