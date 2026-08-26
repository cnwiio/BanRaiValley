
using System.Collections.Generic;
using Lean.Pool;
using UnityEngine;

public class PlantManager : MonoBehaviour
{
    // [SerializeField] private FarmingGridReference gridReference;
    
    
    private readonly Dictionary<Vector3Int, Plant> _spawnedPlantsByCell = new Dictionary<Vector3Int, Plant>();

    private void OnEnable()
    {
        EventBus<OnPlantingEvent>.Subscribe(OnPlanting);
        EventBus<OnClearPlant>.Subscribe(OnClearPlant);
        EventBus<OnNewDayStartedEvent>.Subscribe(OnNewDay);
    }

    private void OnDisable()
    {
        EventBus<OnPlantingEvent>.Unsubscribe(OnPlanting);
        EventBus<OnClearPlant>.Unsubscribe(OnClearPlant);
        EventBus<OnNewDayStartedEvent>.Unsubscribe(OnNewDay);
    }

    
    private void OnPlanting(OnPlantingEvent evt)
    {
        GameObject go = SpawnPrefabs(evt.Prefab, evt.Position);
        RegisterSpawnedPrefabs(go, evt.CellPos, evt.PlantData);
    }

    private void OnClearPlant(OnClearPlant evt)
    {
        DespawnPrefabs(evt.CellPos);
    }
    
    private void OnNewDay(OnNewDayStartedEvent evt)
    {
        GrowAllPlant();   
    }

    #region Spawn And Despawn
    private GameObject SpawnPrefabs(GameObject prefab, Vector3 position)
    {
        return LeanPool.Spawn(prefab, position, Quaternion.identity);
    }
    
    private void DespawnPrefabs(Vector3Int cellPos)
    {
        if (!_spawnedPlantsByCell.TryGetValue(cellPos, out var plant)) return;

        LeanPool.Despawn(plant);
        UnRegisterSpawnedPrefabs(cellPos);
    }
    #endregion
    #region Register 
    private void RegisterSpawnedPrefabs(GameObject prefab, Vector3Int cellPos, PlantData plantData)
    {
        _spawnedPlantsByCell[cellPos] = prefab.GetComponent<Plant>();;
        _spawnedPlantsByCell[cellPos].Initialize(plantData);
    }

    private void UnRegisterSpawnedPrefabs(Vector3Int position)
    {
        _spawnedPlantsByCell.Remove(position);
    }
    #endregion

    private void GrowAllPlant()
    {
        foreach (var plant in _spawnedPlantsByCell.Values)
        {
            plant.Grow();
        }
    }

}
