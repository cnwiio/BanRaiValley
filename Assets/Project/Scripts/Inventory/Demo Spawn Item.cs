using UnityEngine;

public class DemoSpawnItem : MonoBehaviour
{
    public InventoyModel inventoyModel;
    public HotbarInventoryModel hotbarModel;
    public Item[] itemToSpawn;

    public void Start()
    {
        foreach (var item in itemToSpawn)
        {
            hotbarModel.TryAddItem(item, 1);
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
