using System;
using Lean.Pool;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerHandVisualizer : MonoBehaviour
{
    [SerializeField] private Transform spawnTransform;
    [SerializeField] private Item bareHand;
    
    private SlotData currentSlotData;
    private GameObject currentSpawnedPrefab;
    private Item currentSpawnedItem;

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
        
        // check if next spawn item is the same one player have
        if (currentSpawnedItem != null)
        {
            if (currentSlotData.IsEmpty)
            {
                if (currentSpawnedItem == bareHand)
                {
                    return;
                }
            }
            if (currentSpawnedItem == currentSlotData.item)
            {
                return;
            }
        }
        
        SpawnSlotItem(currentSlotData);
    }

    private void SpawnSlotItem(SlotData slotdata)
    {
        DeSpawnCurrentItem();
        
        if (!slotdata.IsEmpty)
        {
            if (slotdata.count == 0) return;
            spawnTransform.localPosition = intialSpawnPostion;
            spawnTransform.localPosition += currentSlotData.item.spawnOffset;
            currentSpawnedItem = slotdata.item;
            currentSpawnedPrefab = LeanPool.Spawn(slotdata.item.prefab, spawnTransform);
            // if (slotdata.item.type == ItemType.Tool)
            // {
            //     spawnTransform.localPosition = intialSpawnPostion;
            //     spawnTransform.localPosition += currentSlotData.item.spawnOffset;
            //     currentItem = LeanPool.Spawn(slotdata.item.prefab, spawnTransform);
            // } else if (slotdata.item.type == ItemType.Seed)
            // {
            //     spawnTransform.localPosition = intialSpawnPostion;
            //     spawnTransform.localPosition += currentSlotData.item.spawnOffset;
            //     currentItem = LeanPool.Spawn(slotdata.item.prefab, spawnTransform);
            // }
        }
        else
        {
            spawnTransform.localPosition = intialSpawnPostion;
            spawnTransform.localPosition += bareHand.spawnOffset;
            currentSpawnedItem = bareHand;
            currentSpawnedPrefab = LeanPool.Spawn(bareHand.prefab, spawnTransform);
        }
    }

    private void DeSpawnCurrentItem()
    {
        if (currentSpawnedPrefab != null)
        {
            LeanPool.Despawn(currentSpawnedPrefab);
            currentSpawnedPrefab = null;
            currentSpawnedItem = null;
        }
    }
}
