using System;
using System.Collections.Generic;
using Lean.Pool;
using UnityEngine;

public class DirtManager : MonoBehaviour
{
    #region Inspector Fields

    [Header("Settings")]
    [SerializeField] private FarmingGridReference gridReference;
    [SerializeField] private int daysToDisappear = 3;

    #endregion


    #region Private Fields

    private readonly Dictionary<Vector3Int, Dirt> _spawnedTilesByCell = new Dictionary<Vector3Int, Dirt>();
    private readonly List<Vector3Int> _tilesToDespawn = new List<Vector3Int>();

    private IFarmingGrid _grid;

    #endregion


    #region Unity Messages

    private void Start()
    {
        EnsureGridReference();
    }

    private void OnEnable()
    {
        EventBus<OnTillingImpactEvent>.Subscribe(OnTillingImpact);
        EventBus<OnTileClearEvent>.Subscribe(OnTileClear);
        EventBus<OnWateringEvent>.Subscribe(OnWatering);
        EventBus<OnPlantingEvent>.Subscribe(OnPlanting);
        EventBus<OnNewDayStartedEvent>.Subscribe(OnNewDay);
    }

    private void OnDisable()
    {
        EventBus<OnTillingImpactEvent>.Unsubscribe(OnTillingImpact);
        EventBus<OnTileClearEvent>.Unsubscribe(OnTileClear);
        EventBus<OnWateringEvent>.Unsubscribe(OnWatering);
        EventBus<OnPlantingEvent>.Unsubscribe(OnPlanting);
        EventBus<OnNewDayStartedEvent>.Unsubscribe(OnNewDay);
    }

    #endregion


    #region Event Handlers

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

    private void OnPlanting(OnPlantingEvent evt)
    {
        if (_spawnedTilesByCell.TryGetValue(evt.CellPos, out var dirt))
        {
            dirt.ResetInactiveDays();
        }
    }

    private void OnNewDay(OnNewDayStartedEvent evt)
    {
        EnsureGridReference();
        _tilesToDespawn.Clear();

        foreach (var kvp in _spawnedTilesByCell)
        {
            Vector3Int cellPos = kvp.Key;
            Dirt soil = kvp.Value;

            bool isPlanted = _grid != null && _grid.IsPlanted(soil.transform.position, out _);

            if (isPlanted)
            {
                // Protected by plant: reset inactive counter
                soil.ResetInactiveDays();
            }
            else
            {
                // Unplanted dirt: increment inactive day counter
                soil.IncrementInactiveDays();

                if (soil.InactiveDays >= daysToDisappear)
                {
                    _tilesToDespawn.Add(cellPos);
                    continue;
                }
            }

            // Unwater surviving soil visual and grid data
            if (_grid != null && _grid.TryUnWatered(soil.transform.position))
            {
                soil.ResetWateredVisual();
            }
        }

        // Cleanly untill the grid and despawn expired dirt tiles
        for (int i = 0; i < _tilesToDespawn.Count; i++)
        {
            Vector3Int cellPos = _tilesToDespawn[i];
            if (_spawnedTilesByCell.TryGetValue(cellPos, out var soil))
            {
                if (_grid != null)
                {
                    _grid.TryUntill(soil.transform.position, out _);
                }
                LeanPool.Despawn(soil);
                UnRegisterSpawnedPrefabs(cellPos);
            }
        }
    }

    #endregion


    #region Spawn And Despawn

    private GameObject SpawnPrefabs(GameObject prefab, Vector3 position, float yRotation)
    {
        return LeanPool.Spawn(prefab, position, Quaternion.Euler(0f, yRotation, 0f));
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
        var dirt = prefab.GetComponent<Dirt>();
        dirt.ResetInactiveDays();
        _spawnedTilesByCell[cellPos] = dirt;
    }

    private void UnRegisterSpawnedPrefabs(Vector3Int position)
    {
        _spawnedTilesByCell.Remove(position);
    }

    #endregion


    #region Private Helpers

    private void EnsureGridReference()
    {
        if (_grid == null && gridReference != null)
        {
            _grid = gridReference.Grid;
        }
    }

    private void WateringSoil(Vector3Int cellPos)
    {
        if (_spawnedTilesByCell.TryGetValue(cellPos, out var value))
        {
            value.Watering();
            value.ResetInactiveDays();
        }
    }

    #endregion
}
