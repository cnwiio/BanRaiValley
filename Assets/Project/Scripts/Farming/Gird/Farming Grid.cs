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

        if (!IsWaterable(cellPos)) return false;
        
        RegisterPlantedSoil(cellPos);
        return true;
    }

    #endregion
    
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

    private void UnRegisterTiledSoil(Vector3Int cellPos) => _tileStore.SetTillable(cellPos);
    private void RegisterWateredSoil(Vector3Int cellPos) => _tileStore.SetWatered(cellPos);
    private void RegisterPlantedSoil(Vector3Int cellPos) => _tileStore.SetPlanted(cellPos);
    

}