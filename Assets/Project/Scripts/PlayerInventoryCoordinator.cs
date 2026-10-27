using UnityEngine;

/// <summary>
/// Listens for OnItemPickupEvent (e.g. raised by AIBrain on death) and grants
/// the item directly to the player: hotbar first, then overflow into the
/// main inventory, Minecraft-style auto-pickup.
/// </summary>
public class PlayerInventoryCoordinator : MonoBehaviour
{
    [SerializeField] private HotbarInventoryModel hotbar;
    [SerializeField] private InventoyModel inventory;

    private void OnEnable()
    {
        EventBus<OnItemPickupEvent>.Subscribe(OnItemPickup);
    }

    private void OnDisable()
    {
        EventBus<OnItemPickupEvent>.Unsubscribe(OnItemPickup);
    }

    private void OnItemPickup(OnItemPickupEvent evt)
    {
        GrantItem(evt.item, evt.amount);
    }

    public void GrantItem(Item item, int amount)
    {
        if (item == null || amount <= 0) return;

        // 1. Prefer stacking onto an existing matching stack already in the
        //    main inventory (never touches empty slots here).
        int leftover = inventory.StackExistingGetLeftover(item, amount);

        // 2. Whatever didn't stack goes to the hotbar (fills its own matching
        //    stacks first, then its empty slots).
        if (leftover > 0)
        {
            leftover = hotbar.AddItemGetLeftover(item, leftover);
            hotbar.CheckSelectedSlotChanged();
        }

        // 3. Anything still left (hotbar full) spills into inventory's empty slots.
        if (leftover > 0)
        {
            leftover = inventory.AddItemGetLeftover(item, leftover);
        }

        EventBus<InventoryUIRefreshEvent>.Raise(new InventoryUIRefreshEvent());

        if (leftover > 0)
        {
            // Both inventories are full. Extend this to spawn a world pickup
            // for the remainder if you want a physical fallback.
            Debug.Log($"Inventory full: {leftover}x {item.name} could not be picked up.");
        }
    }
}