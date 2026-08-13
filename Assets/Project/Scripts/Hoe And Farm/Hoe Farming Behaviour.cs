using System;
using System.Collections.Generic;
using Lean.Pool;
using UnityEngine;

public class HoeFarmingBehaviour : MonoBehaviour
{
    private readonly Dictionary<Vector3Int, GameObject> spawnedPrefabsList = new Dictionary<Vector3Int, GameObject>();

    #region Bind Events
    private void OnEnable()
    {
        EventBus<OnTillingImpactEvent>.Subscribe(OnTillingImpact);
        EventBus<OnTileClearEvent>.Subscribe(OnTileClear);
    }

    private void OnDisable()
    {
        EventBus<OnTillingImpactEvent>.Unsubscribe(OnTillingImpact);
        EventBus<OnTileClearEvent>.Unsubscribe(OnTileClear);
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
    #endregion
    #region Spawn And Despawn
    private GameObject SpawnPrefabs(GameObject prefab, Vector3 position, float Yrotation)
    {
        return LeanPool.Spawn(prefab, position, Quaternion.Euler(0f, Yrotation, 0f));
    }
    private void DespawnPrefabs(Vector3Int position)
    {
        if (GetSpawnedPrefabs(position) == null) return;

        LeanPool.Despawn(spawnedPrefabsList[position]);
        UnRegisterSpawnedPrefabs(position);
    }
    #endregion
    #region Register 
    private void RegisterSpawnedPrefabs(GameObject prefab, Vector3Int position)
    {
        spawnedPrefabsList.Add(position, prefab);
    }

    private void UnRegisterSpawnedPrefabs(Vector3Int position)
    {
        spawnedPrefabsList.Remove(position);
    }

    private GameObject GetSpawnedPrefabs(Vector3Int position)
    {
        spawnedPrefabsList.TryGetValue(position, out var value);
        return value;
    }
    #endregion
}
