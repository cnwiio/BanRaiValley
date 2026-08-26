using UnityEngine;

/// <summary>
/// Contract any tool (Hoe today, WateringCan/Seeder tomorrow) uses to interact with
/// a farming grid. Depending on this interface instead of the FarmingGrid class
/// directly is what makes the two swappable/testable/decoupled (DIP).
/// </summary>
public interface IFarmingGrid
{
    bool IsValidForTilling(Vector3 worldPos, out Vector3 cellWorldPos);
    bool IsTilled(Vector3 worldPos, out Vector3 cellWorldPos);
    bool IsWaterable(Vector3 worldPos, out Vector3 cellWorldPos);
    bool IsPlanted(Vector3 worldPos, out Vector3 cellWorldPos);
    bool IsPlantable(Vector3 worldPos, out Vector3 cellWorldPos);
    bool TryTill(Vector3 worldPos, out Vector3Int cellPos);
    bool TryUntill(Vector3 worldPos, out Vector3Int cellPos);
    bool TryWatering(Vector3 worldPos, out Vector3Int cellPos);
    bool TryPlanting(Vector3 worldPos, out Vector3Int cellPos);
    bool TryClearPlant(Vector3 worldPos, out Vector3Int cellPos);

    /// <summary>
    /// Returns true if the tile at the given cell position was watered during the current day.
    /// Used by <see cref="CropGrowthManager"/> to evaluate growth eligibility per crop.
    /// </summary>
    /// <param name="cellPos">The tilemap cell coordinate to check.</param>
    /// <returns>True if the tile is currently in a watered state.</returns>
    bool IsWatered(Vector3Int cellPos);

    /// <summary>
    /// Resets all watered tiles to dry for the start of a new day.
    /// Must be called <em>after</em> growth evaluation so watered state is still readable during the tick.
    /// </summary>
    /// <returns>The number of tiles that had their hydration cleared.</returns>
    int ResetDailyHydration();
}
