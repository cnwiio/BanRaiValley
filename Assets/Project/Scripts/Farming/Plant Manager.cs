using System;
using System.Collections.Generic;
using Lean.Pool;
using UnityEngine;

public class PlantManager : MonoBehaviour
{
    private readonly Dictionary<Vector3Int, GameObject> _spawnedPlantsByCell = new Dictionary<Vector3Int, GameObject>();

    private void OnEnable()
    {
        EventBus<OnPlantingEvent>.Subscribe(OnPlanting);
        EventBus<OnClearPlant>.Subscribe(OnClearPlant);
    }

    private void OnDisable()
    {
        EventBus<OnPlantingEvent>.Unsubscribe(OnPlanting);
        EventBus<OnClearPlant>.Unsubscribe(OnClearPlant);
    }

    private void OnPlanting(OnPlantingEvent evt)
    {
        GameObject go = SpawnPrefabs(evt.Prefab, evt.Position);
        RegisterSpawnedPrefabs(go, evt.CellPos);
    }

    private void OnClearPlant(OnClearPlant evt)
    {
        DespawnPrefabs(evt.CellPos);
    }

    #region Spawn And Despawn
    private GameObject SpawnPrefabs(GameObject prefab, Vector3 position)
    {
        return LeanPool.Spawn(prefab, position, Quaternion.identity);
    }
    
    private void DespawnPrefabs(Vector3Int cellPos)
    {
        if (!_spawnedPlantsByCell.TryGetValue(cellPos, out var go)) return;

        LeanPool.Despawn(go);
        UnRegisterSpawnedPrefabs(cellPos);
    }
    #endregion
    #region Register 
    private void RegisterSpawnedPrefabs(GameObject prefab, Vector3Int cellPos)
    {
        _spawnedPlantsByCell[cellPos] = prefab;
    }

    private void UnRegisterSpawnedPrefabs(Vector3Int position)
    {
        _spawnedPlantsByCell.Remove(position);
    }
    #endregion


}
