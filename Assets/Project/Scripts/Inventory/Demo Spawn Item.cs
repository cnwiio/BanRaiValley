using UnityEngine;

public class DemoSpawnItem : MonoBehaviour
{
    public InventoyModel inventoyModel;
    public Item[] itemToSpawn;

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
