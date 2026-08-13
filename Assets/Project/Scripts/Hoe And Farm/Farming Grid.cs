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

    [SerializeField] private Grid grid;
    [SerializeField] private LayerMask obstacleLayerMask;
    [SerializeField] private FarmingGridReference farmingGridReference;

    private readonly Dictionary<Vector3Int, TileState> _overrides = new Dictionary<Vector3Int, TileState>();
    private const float CELL_SIZE_MODIFIER = 0.48f;
    private float currentYRotation = 0;

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

        // Rotation is display/preview feedback only (not core grid state), so it's
        // still fine for this to stay a broadcast event.
        //EventBus<OnRotateFarmEvent>.Subscribe(OnRotateFarm);
    }

    private void OnDisable()
    {
        if (farmingGridReference != null)
            farmingGridReference.Unregister(this);

        //EventBus<OnRotateFarmEvent>.Unsubscribe(OnRotateFarm);
    }

    #endregion

    //private void OnRotateFarm(OnRotateFarmEvent evt)
    //{
    //    currentYRotation = evt.YRotation;
    //}

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

    #endregion

    /// <summary>
    /// คืนค่า true ถ้าช่องนี้อยู่ในโซนและยังไม่ได้พรวน / ไม่มีสิ่งกีดขวาง
    /// </summary>
    private bool IsValidForTilling(Vector3Int cellPos, Vector3 cellWorldPos)
    {
        if (GetTileState(cellPos) != TileState.Tillable) return false;
        return !Physics.CheckBox(cellWorldPos, grid.cellSize * CELL_SIZE_MODIFIER, Quaternion.identity, obstacleLayerMask);
    }

    public void RegisterTilledSoil(Vector3Int gridPos) => _overrides[gridPos] = TileState.Tilled;

    public void UnRegisterTiledSoil(Vector3Int gridPos) => _overrides[gridPos] = TileState.Tillable;

    public TileState GetTileState(Vector3Int gridPos)
    {
        return _overrides.TryGetValue(gridPos, out var state) ? state : TileState.Tillable;
    }
}
