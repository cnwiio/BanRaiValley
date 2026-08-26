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
    bool IsWatered(Vector3 worldPos);
    
    bool IsPlantable(Vector3 worldPos, out Vector3 cellWorldPos);
    bool IsPlanted(Vector3 worldPos, out Vector3 cellWorldPos);
    
    bool TryTill(Vector3 worldPos, out Vector3Int cellPos);
    bool TryUntill(Vector3 worldPos, out Vector3Int cellPos);
    
    bool TryWatering(Vector3 worldPos, out Vector3Int cellPos);
    bool TryUnWatered(Vector3 worldPos);
    
    bool TryPlanting(Vector3 worldPos, out Vector3Int cellPos);
    bool TryClearPlant(Vector3 worldPos, out Vector3Int cellPos);
}
