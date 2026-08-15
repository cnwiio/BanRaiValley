using UnityEngine;

public class FarmingGrid : MonoBehaviour, IFarmingGrid
{
    [SerializeField] private Grid grid;
    [SerializeField] private LayerMask obstacleLayerMask;
    [SerializeField] private FarmingGridReference farmingGridReference;

    private readonly ITileStore _tileStore = new TileStore();
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
        return _tileStore.IsValidForWatering(cellPos);
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

        if (GetTileState(cellPos) != TileState.Tilled) return false;

        UnRegisterTiledSoil(cellPos);
        return true;
    }

    public bool TryWater(Vector3 worldPos, out Vector3Int cellPos)
    {
        cellPos = grid.WorldToCell(worldPos);

        // var watered = _tileStore.TryWater(cellPos);
        // Debug.Log(_tileStore.IsWatered(cellPos));
        return _tileStore.TryWater(cellPos);
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

    public void RegisterTilledSoil(Vector3Int cellPos) => _tileStore.SetTilled(cellPos);

    public void UnRegisterTiledSoil(Vector3Int cellPos) => _tileStore.SetTillable(cellPos);

    public TileState GetTileState(Vector3Int cellPos) => _tileStore.GetState(cellPos);

    public void WateringTile(Vector3Int cellPos) => _tileStore.TryWater(cellPos);
}