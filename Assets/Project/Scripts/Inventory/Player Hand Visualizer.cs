using Lean.Pool;
using UnityEngine;

public class PlayerHandVisualizer : MonoBehaviour
{
    private SlotData currentSlotData;
    private GameObject currentItem;
    [SerializeField] private Transform spawnTransform;
    private void OnEnable()
    {
        EventBus<OnHotbarChangeEvent>.Subscribe(SpawnSlotItem);
    }

    private void OnDisable()
    {
        EventBus<OnHotbarChangeEvent>.Unsubscribe(SpawnSlotItem);
    }

    private void SpawnSlotItem(OnHotbarChangeEvent evt)
    {
        //if (evt.slotData == currentSlotData) return;
        currentSlotData = evt.slotData;
        SpawnSlotItem(currentSlotData);
    }

    private void SpawnSlotItem(SlotData slotdata)
    {
        if (currentItem != null)
        {
            LeanPool.Despawn(currentItem);
            currentItem = null;
        }
        if (!slotdata.IsEmpty)
        {
            currentItem = LeanPool.Spawn(slotdata.item.prefab, spawnTransform);
        }
    }    
}
