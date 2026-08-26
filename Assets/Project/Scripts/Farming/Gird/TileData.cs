using System.Collections.Generic;
using UnityEngine;

public enum TileState
{
    Untillable,   // พื้นที่นอกโซน / มีสิ่งกีดขวาง ปลูกไม่ได้
    Tillable,     // ดินว่าง พรวนได้
    Tilled        // พรวนแล้ว
}

/// <summary>
/// Immutable tile data. No in-place mutation allowed, so callers are forced
/// to write the new value back into storage explicitly (this is what fixes
/// the "struct copy in dictionary" bug from the original implementation).
/// </summary>
public readonly struct TileData
{
    public bool IsTilled { get; }
    public bool IsWatered { get; }
    public bool IsPlanted { get; }

    public TileData(bool isTilled, bool isWatered, bool isPlantable)
    {
        IsTilled = isTilled;
        IsWatered = isWatered;
        IsPlanted = isPlantable;
    }

    public TileData WithTilled(bool value) => new TileData(value, IsWatered, IsPlanted);
    public TileData WithWatered(bool value) => new TileData(IsTilled, value, IsPlanted);
    public TileData WithPlatable(bool value) => new TileData(IsTilled, IsWatered, value);
}

/// <summary>
/// Owns tile state storage and the rules for changing it.
/// Knows nothing about Unity physics/grid world-space - purely logical state.
/// </summary>
public interface ITileStore
{
    bool IsTilled(Vector3Int cellPos);
    bool IsWatered(Vector3Int cellPos);
    bool IsPlanted(Vector3Int cellPos);
    void SetTilled(Vector3Int cellPos);
    void SetUnTill(Vector3Int cellPos);
    void SetWatered(Vector3Int cellPos);
    void SetUnWatered(Vector3Int cellPos);
    void SetPlanted(Vector3Int cellPos);
    void SetUnPlant(Vector3Int cellPos);
}

public class TileStore : ITileStore
{
    private readonly Dictionary<Vector3Int, TileData> _tiles = new Dictionary<Vector3Int, TileData>();

    public bool IsTilled(Vector3Int cellPos) =>
        _tiles.TryGetValue(cellPos, out var tile) && tile.IsTilled;

    public bool IsWatered(Vector3Int cellPos) =>
        _tiles.TryGetValue(cellPos, out var tile) && tile.IsWatered;

    public bool IsPlanted(Vector3Int cellPos)  => 
        _tiles.TryGetValue(cellPos, out var tile) && tile.IsPlanted;


    public void SetTilled(Vector3Int cellPos) =>
        _tiles[cellPos] = new TileData(true, false, false);

    public void SetUnTill(Vector3Int cellPos) =>
        _tiles[cellPos] = new TileData(false, false, false);

    public void SetWatered(Vector3Int cellPos)
    {
        var current = _tiles.TryGetValue(cellPos, out var tile)
            ? tile
            : new TileData(true, false, false);
        
        _tiles[cellPos] = current.WithWatered(true);
    }
    public void SetUnWatered(Vector3Int cellPos)
    {
        var current = _tiles.TryGetValue(cellPos, out var tile)
            ? tile
            : new TileData(true, false, false);
        
        _tiles[cellPos] = current.WithWatered(false);
    }
    
    public void SetPlanted(Vector3Int cellPos)
    {
        var current = _tiles.TryGetValue(cellPos, out var tile)
            ? tile
            : new TileData(true, false, false);
        
        _tiles[cellPos] = current.WithPlatable(true);
    }
    public void SetUnPlant(Vector3Int cellPos)
    {
        var current = _tiles.TryGetValue(cellPos, out var tile)
            ? tile
            : new TileData(true, false, false);
        
        _tiles[cellPos] = current.WithPlatable(false);
    }
}