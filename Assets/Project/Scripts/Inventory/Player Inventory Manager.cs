using UnityEngine;

public class PlayerInventoryManager : MonoBehaviour
{
    [SerializeField] private HotbarInventoryModel hotbarInventory;
    // [SerializeField] private BaseInventory ;
    
    
    
    private void UseConsumable()
    {
        hotbarInventory.UseConsumableInSelectedSlot();
    }
}
