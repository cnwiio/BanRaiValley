using System;
using Lean.Pool;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerHandVisualizer : MonoBehaviour
{
    private SlotData currentSlotData;
    private GameObject currentItem;
    [SerializeField] private Transform spawnTransform;

    private Vector3 intialSpawnPostion;
    private void Awake()
    {
        intialSpawnPostion = spawnTransform.transform.localPosition;
    }

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
            // spawnTransform.localPosition = intialSpawnTransform.localPosition;
        }
        if (!slotdata.IsEmpty)
        {
            spawnTransform.localPosition = intialSpawnPostion;
            spawnTransform.localPosition += currentSlotData.item.spawnOffset;
            currentItem = LeanPool.Spawn(slotdata.item.prefab, spawnTransform, false);
            // currentItem = Instantiate(slotdata.item.prefab, spawnTransform);
            // currentItem.transform.position = Vector3.zero;
            // currentItem.transform.position += slotdata.item.spawnOffset;
        }
    }    
}
