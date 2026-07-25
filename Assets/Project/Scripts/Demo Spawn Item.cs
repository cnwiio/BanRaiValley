using UnityEngine;

public class DemoSpawnItem : MonoBehaviour
{
    public InventoryManager inventoryManager;
    public Item[] itemToSpawn;

    public void SpawnItem(int id)
    {
        inventoryManager?.AddItem(itemToSpawn[id]);
    }
}
