using UnityEngine;

public class DemoSpawnItem : MonoBehaviour
{
    public InventoryManager inventoryManager;
    public InventoyModel inventoyModel;
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
    public void SpawnItem2(int id)
    {
        bool result = inventoyModel.TryAddItem(itemToSpawn[id], 3);
        if (result)
        {
            //Debug.Log("Inventory added");
            //EventBus<InventoryRefreshEvent>.Raise(new InventoryRefreshEvent() { });
        } else
        {
            //Debug.Log("Inventory is full");
        }
    }
}
