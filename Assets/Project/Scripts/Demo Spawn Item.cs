using UnityEngine;

public class DemoSpawnItem : MonoBehaviour
{
    public InventoryManager inventoryManager;
    public Item[] itemToSpawn;

    public void SpawnItem(int id)
    {
        bool result = inventoryManager.AddItem(itemToSpawn[id]);
        if (result)
        {
            Debug.Log("Inventory added");
        } else
        {
            Debug.Log("Inventory is full");
        }
    }
}
