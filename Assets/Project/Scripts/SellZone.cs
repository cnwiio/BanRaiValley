using UnityEngine;

/// <summary>
/// Trigger zone that, on player entry, sells all items of a given ItemType
/// (default: Plant) from the player's inventory. Sell price is read from
/// Item.value, and proceeds are added to PlayerMoney.
///
/// Setup:
/// - Add a Collider to this GameObject and check "Is Trigger".
/// - Player object needs a Rigidbody (can be kinematic) for OnTriggerEnter to fire.
/// - Assign the player's BaseInventory (e.g. InventoyModel) and PlayerMoney
///   in the inspector, OR leave them empty and this script will try to find
///   them on the object that entered the trigger (see useComponentLookup).
/// </summary>
public class SellZone : MonoBehaviour
{
    [Header("What to sell")]
    [SerializeField] private ItemType sellItemType = ItemType.Plant;

    [Header("Player references")]
    [SerializeField] private BaseInventory[] inventory;
    [SerializeField] private PlayerMoney playerMoney;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (inventory == null || playerMoney == null) return;
        
        for (int i = 0; i < inventory.Length; i++)
        {
            SellItemsByType(inventory[i], playerMoney, sellItemType);
        }
    }

    private void SellItemsByType(BaseInventory targetInventory, PlayerMoney targetMoney, ItemType type)
    {
        int totalEarned = 0;
        int itemsSold = 0;

        for (int i = 0; i < targetInventory.TotalSlot; i++)
        {
            SlotData slot = targetInventory.GetSlotData(i);
            if (slot.IsEmpty || slot.item == null) continue;
            if (slot.item.type != type) continue;

            totalEarned += slot.item.value * slot.count;
            itemsSold += slot.count;
            targetInventory.ClearSlot(i);
        }

        if (itemsSold > 0)
        {
            targetMoney.AddMoney(totalEarned);
            if (targetInventory is HotbarInventoryModel horbarInventory)
            {
                horbarInventory.CheckSelectedSlotChanged();
            }
            EventBus<InventoryUIRefreshEvent>.Raise(new InventoryUIRefreshEvent());
            Debug.Log($"Sold {itemsSold}x {type} for {totalEarned}$");
        }
    }
}