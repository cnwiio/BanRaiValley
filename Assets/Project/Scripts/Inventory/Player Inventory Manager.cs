using System;
using UnityEngine;

public class PlayerInventoryManager : MonoBehaviour
{
    [SerializeField] private HotbarInventoryModel hotbarInventory;
    // [SerializeField] private BaseInventory ;

    private void OnEnable()
    {
        EventBus<OnPlantingEvent>.Subscribe(OnPlanting);
    }

    private void OnDisable()
    {
        EventBus<OnPlantingEvent>.Unsubscribe(OnPlanting);
    }

    private void OnPlanting(OnPlantingEvent evt)
    {
        UseConsumable();
    }
    
    private void UseConsumable()
    {
        hotbarInventory.UseConsumableInSelectedSlot();
    }
}
