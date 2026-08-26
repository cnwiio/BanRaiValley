using UnityEditorInternal;
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
        return IsTilled(cellPos);
    }

    public bool IsWaterable(Vector3 worldPos, out Vector3 cellWorldPos)
    {
        var cellPos = grid.WorldToCell(worldPos);
        cellWorldPos = grid.GetCellCenterWorld(cellPos);
        return IsWaterable(cellPos);
    }

    public bool IsPlanted(Vector3 worldPos, out Vector3 cellWorldPos)
    {
        var cellPos = grid.WorldToCell(worldPos);
        cellWorldPos = grid.GetCellCenterWorld(cellPos);
        return _tileStore.IsPlanted(cellPos);
    }

    public bool IsPlantable(Vector3 worldPos, out Vector3 cellWorldPos)
    {
        var cellPos = grid.WorldToCell(worldPos);
        cellWorldPos = grid.GetCellCenterWorld(cellPos);
        return IsPlantable(cellPos);
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

        if (!IsTilled(cellPos)) return false;

        UnRegisterTiledSoil(cellPos);
        return true;
    }

    public bool TryWatering(Vector3 worldPos, out Vector3Int cellPos)
    {
        cellPos = grid.WorldToCell(worldPos);

        if (!IsWaterable(cellPos)) return false;

        RegisterWateredSoil(cellPos);
        return true;
    }

    public bool TryPlanting(Vector3 worldPos, out Vector3Int cellPos)
    {
        cellPos = grid.WorldToCell(worldPos);

        if (!IsPlantable(cellPos)) return false;

        RegisterPlantedSoil(cellPos);
        return true;
    }

    public bool TryClearPlant(Vector3 worldPos, out Vector3Int cellPos)
    {
        cellPos = grid.WorldToCell(worldPos);

        if (!_tileStore.IsPlanted(cellPos)) return false;

        UnRegisterPlantedSoil(cellPos);
        return true;
    }

    /// <summary>
    /// Returns true if the tile at <paramref name="cellPos"/> was watered during the current day.
    /// Delegates directly to <see cref="ITileStore.IsWatered(Vector3Int)"/>.
    /// </summary>
    /// <param name="cellPos">The tilemap cell coordinate to query.</param>
    public bool IsWatered(Vector3Int cellPos) => _tileStore.IsWatered(cellPos);

    /// <summary>
    /// Resets all watered tiles to dry for the new day.
    /// Delegates to <see cref="ITileStore.ResetDailyHydration"/>.
    /// </summary>
    /// <returns>The number of tiles whose hydration was cleared.</returns>
    public int ResetDailyHydration() => _tileStore.ResetDailyHydration();

    #endregion

    #region Private Methods

    private bool IsTilled(Vector3Int cellPos) => _tileStore.IsTilled(cellPos);

    private bool IsValidForTilling(Vector3Int cellPos, Vector3 cellWorldPos)
    {
        if (IsTilled(cellPos)) return false;
        return !Physics.CheckBox(cellWorldPos, grid.cellSize * CELL_SIZE_MODIFIER, Quaternion.identity, obstacleLayerMask);
    }

    private bool IsWaterable(Vector3Int cellPos)
    {
        if (!IsTilled(cellPos)) return false;
        return !_tileStore.IsWatered(cellPos);
    }

    private bool IsPlantable(Vector3Int cellPos)
    {
        if (!IsTilled(cellPos)) return false;
        return !_tileStore.IsPlanted(cellPos);
    }

    private void RegisterTilledSoil(Vector3Int cellPos) => _tileStore.SetTilled(cellPos);
    private void UnRegisterTiledSoil(Vector3Int cellPos) => _tileStore.SetUnTill(cellPos);
    private void RegisterWateredSoil(Vector3Int cellPos) => _tileStore.SetWatered(cellPos);
    private void RegisterPlantedSoil(Vector3Int cellPos) => _tileStore.SetPlanted(cellPos);
    private void UnRegisterPlantedSoil(Vector3Int cellPos) => _tileStore.SetUnPlant(cellPos);

    #endregion
}