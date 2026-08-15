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
    public TileState State { get; }
    public bool IsWatered { get; }

    public TileData(TileState state, bool isWatered)
    {
        State = state;
        IsWatered = isWatered;
    }

    public TileData WithWatered(bool value) => new TileData(State, value);
    public TileData WithState(TileState state) => new TileData(state, IsWatered);
}

/// <summary>
/// Owns tile state storage and the rules for changing it.
/// Knows nothing about Unity physics/grid world-space - purely logical state.
/// </summary>
public interface ITileStore
{
    TileState GetState(Vector3Int cellPos);
    bool IsWatered(Vector3Int cellPos);
    bool IsValidForWatering(Vector3Int cellPos);
    void SetTilled(Vector3Int cellPos);
    void SetTillable(Vector3Int cellPos);
    bool TryWater(Vector3Int cellPos);
}

public class TileStore : ITileStore
{
    private readonly Dictionary<Vector3Int, TileData> _tiles = new Dictionary<Vector3Int, TileData>();

    public TileState GetState(Vector3Int cellPos) =>
        _tiles.TryGetValue(cellPos, out var tile) ? tile.State : TileState.Tillable;

    public bool IsWatered(Vector3Int cellPos) =>
        _tiles.TryGetValue(cellPos, out var tile) && tile.IsWatered;

    public bool IsValidForWatering(Vector3Int cellPos)
    {
        if (GetState(cellPos) != TileState.Tilled) return false;
        return !IsWatered(cellPos);
    }

    public void SetTilled(Vector3Int cellPos) =>
        _tiles[cellPos] = new TileData(TileState.Tilled, false);

    public void SetTillable(Vector3Int cellPos) =>
        _tiles[cellPos] = new TileData(TileState.Tillable, false);

    public bool TryWater(Vector3Int cellPos)
    {
        if (!IsValidForWatering(cellPos)) return false;

        // Read -> mutate copy -> write back, since TileData is immutable.
        var current = _tiles.TryGetValue(cellPos, out var tile)
            ? tile
            : new TileData(TileState.Tilled, false);

        _tiles[cellPos] = current.WithWatered(true);
        return true;
    }
}