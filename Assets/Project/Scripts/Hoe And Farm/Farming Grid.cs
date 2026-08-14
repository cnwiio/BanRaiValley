using Lean.Pool;
using System.Collections.Generic;
using System.Xml;
using TMPro;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;


public class FarmingGrid : MonoBehaviour, IFarmingGrid
{
    public enum TileState
    {
        Untillable,   // พื้นที่นอกโซน / มีสิ่งกีดขวาง ปลูกไม่ได้
        Tillable,     // ดินว่าง พรวนได้
        Tilled        // พรวนแล้ว
    }

    public struct TileData
    {
        public TileState State;
        public bool IsWatered;

        public void Watering()
        {
            IsWatered = true;
        }
    }

    [SerializeField] private Grid grid;
    [SerializeField] private LayerMask obstacleLayerMask;
    [SerializeField] private FarmingGridReference farmingGridReference;

    private readonly Dictionary<Vector3Int, TileData> _overrides = new Dictionary<Vector3Int, TileData>();
    private const float CELL_SIZE_MODIFIER = 0.48f;

    #region Lifecycle

    private void OnEnable()
    {
        if (farmingGridReference != null)
        {
            farmingGridReference.Register(this);
        }
        else
        {
            Debug.LogWarning($"{name}: FarmingGridReference not assigned - no Hoe will be able to find this grid.");
        }
    }

    private void OnDisable()
    {
        if (farmingGridReference != null)
            farmingGridReference.Unregister(this);
    }

    #endregion

    #region IFarmingGrid

    public bool IsValidForTilling(Vector3 worldPos, out Vector3 cellWorldPos)
    {
        var cellPos = grid.WorldToCell(worldPos);
        cellWorldPos = grid.GetCellCenterWorld(cellPos);
        return IsValidForTilling(cellPos, cellWorldPos);
    }

    public bool IsTilled(Vector3 worldPos, out Vector3 cellWorldPos)
    {
        var cellPos = grid.WorldToCell(worldPos);
        cellWorldPos = grid.GetCellCenterWorld(cellPos);
        return GetTileState(cellPos) == TileState.Tilled;
    }

    public bool IsWaterable(Vector3 worldPos, out Vector3 cellWorldPos)
    {
        var cellPos = grid.WorldToCell(worldPos);
        cellWorldPos = grid.GetCellCenterWorld(cellPos);
        return IsValidForWatering(cellPos);
    }

    public bool TryTill(Vector3 worldPos, out Vector3Int cellPos)
    {
        cellPos = grid.WorldToCell(worldPos);
        var cellWorldPos = grid.GetCellCenterWorld(cellPos);

        if (!IsValidForTilling(cellPos, cellWorldPos)) return false;

        RegisterTilledSoil(cellPos);
        return true;
    }

    public bool TryUntill(Vector3 worldPos, out Vector3Int cellPos)
    {
        cellPos = grid.WorldToCell(worldPos);
        var cellWorldPos = grid.GetCellCenterWorld(cellPos);

        if (GetTileState(cellPos) != TileState.Tilled) return false;

        UnRegisterTiledSoil(cellPos);
        return true;
    }

    public bool TryWater(Vector3 worldPos, out Vector3Int cellPos)
    {
        cellPos = grid.WorldToCell(worldPos);
        var cellWorldPos = grid.GetCellCenterWorld(cellPos);

        if (GetTileState(cellPos) != TileState.Tilled) return false;

        WateringTile(cellPos);
        return true;
    }

    #endregion

    /// <summary>
    /// คืนค่า true ถ้าช่องนี้อยู่ในโซนและยังไม่ได้พรวน / ไม่มีสิ่งกีดขวาง
    /// </summary>
    private bool IsValidForTilling(Vector3Int cellPos, Vector3 cellWorldPos)
    {
        if (GetTileState(cellPos) != TileState.Tillable) return false;
        return !Physics.CheckBox(cellWorldPos, grid.cellSize * CELL_SIZE_MODIFIER, Quaternion.identity, obstacleLayerMask);
    }
    private bool IsValidForWatering(Vector3Int cellPos)
    {
        if (GetTileState(cellPos) != TileState.Tilled) return false;
        return _overrides.TryGetValue(cellPos, out var tileData) ? !tileData.IsWatered : true;
    }

    public void RegisterTilledSoil(Vector3Int cellPos) => _overrides[cellPos] = new TileData() { IsWatered = false, State = TileState.Tilled };

    public void UnRegisterTiledSoil(Vector3Int cellPos) => _overrides[cellPos] = new TileData() { IsWatered = false, State = TileState.Tillable };

    public TileState GetTileState(Vector3Int cellPos)
    {
        return _overrides.TryGetValue(cellPos, out var tileData) ? tileData.State : TileState.Tillable;
    }

    public void WateringTile(Vector3Int cellPos)
    {
        _overrides[cellPos].Watering();
    }
}
