using System;
using System.Collections.Generic;
using Lean.Pool;
using UnityEngine;

public class HoeFarmingBehaviour : MonoBehaviour
{
    [SerializeField] private FarmingGridReference gridReference;
    private readonly Dictionary<Vector3Int, Dirt> _spawnedTilesByCell = new Dictionary<Vector3Int, Dirt>();
    
    private IFarmingGrid _grid;
    private void Start()
    {
        _grid = gridReference.Grid;
    }
    #region Bind Events
    private void OnEnable()
    {
        EventBus<OnTillingImpactEvent>.Subscribe(OnTillingImpact);
        EventBus<OnTileClearEvent>.Subscribe(OnTileClear);
        EventBus<OnWateringEvent>.Subscribe(OnWatering);
        EventBus<OnNewDayStartedEvent>.Subscribe(OnNewDay);
    }

    private void OnDisable()
    {
        EventBus<OnTillingImpactEvent>.Unsubscribe(OnTillingImpact);
        EventBus<OnTileClearEvent>.Unsubscribe(OnTileClear);
        EventBus<OnWateringEvent>.Unsubscribe(OnWatering);
        EventBus<OnNewDayStartedEvent>.Unsubscribe(OnNewDay);
    }

    private void OnTillingImpact(OnTillingImpactEvent evt)
    {
        GameObject go = SpawnPrefabs(evt.prefabs, evt.Position, evt.YRotation);
        RegisterSpawnedPrefabs(go, evt.CellPos);
    }
    private void OnTileClear(OnTileClearEvent evt)
    {
        DespawnPrefabs(evt.CellPos);
    }
    private void OnWatering(OnWateringEvent evt)
    {
        WateringSoil(evt.CellPos);
    }

    private void OnNewDay(OnNewDayStartedEvent evt)
    {
        UnWateredAllSoil();
    }
    #endregion
    #region Spawn And Despawn
    private GameObject SpawnPrefabs(GameObject prefab, Vector3 position, float yrotation)
    {
        return LeanPool.Spawn(prefab, position, Quaternion.Euler(0f, yrotation, 0f));
    }
    private void DespawnPrefabs(Vector3Int cellPos)
    {
        if (!_spawnedTilesByCell.TryGetValue(cellPos, out var go)) return;

        LeanPool.Despawn(go);
        UnRegisterSpawnedPrefabs(cellPos);
    }
    #endregion
    #region Register 
    private void RegisterSpawnedPrefabs(GameObject prefab, Vector3Int cellPos)
    {
        _spawnedTilesByCell[cellPos] = prefab.GetComponent<Dirt>();
    }

    private void UnRegisterSpawnedPrefabs(Vector3Int position)
    {
        _spawnedTilesByCell.Remove(position);
    }

    // private GameObject GetSpawnedPrefabs(Vector3Int position)
    // {
    //     _spawnedTilesByCell.TryGetValue(position, out var value);
    //     return value;
    // }
    #endregion

    private void WateringSoil(Vector3Int cellPos)
    {
        if (_spawnedTilesByCell.TryGetValue(cellPos, out var value))
        {
            value.Watering();
        }
    }

    private void UnWateredAllSoil()
    {
        foreach (var soil in _spawnedTilesByCell.Values)
        {
            if(_grid.TryUnWatered(soil.transform.position))
                soil.ResetWateredVisual();
        }
    }

}
