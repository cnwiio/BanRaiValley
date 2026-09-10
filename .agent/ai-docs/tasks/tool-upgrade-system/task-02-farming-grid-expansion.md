# Task 02: Farming Grid Coordinate Expansion

## 1. Task Goal
Extend the [IFarmingGrid](file:///d:/Work/Unity%20Project/BanRaiValley/Assets/Project/Scripts/Farming/Gird/IFarmingGrid.cs) interface and [Farming Grid.cs](file:///d:/Work/Unity%20Project/BanRaiValley/Assets/Project/Scripts/Farming/Gird/Farming%20Grid.cs) to expose grid cell size and coordinate conversion methods, enabling deterministic multi-cell calculations without floating point accumulation errors.

## 2. Task Information
- **System**: Tool Upgrade System
- **Parent Plan**: [.agent/ai-docs/plan/tool-upgrade-system-plan.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/ai-docs/plan/tool-upgrade-system-plan.md)
- **Target File(s)**:
  - `Assets/Project/Scripts/Farming/Gird/IFarmingGrid.cs` [MODIFY]
  - `Assets/Project/Scripts/Farming/Gird/Farming Grid.cs` [MODIFY]
- **Dependencies / Prerequisites**:
  - Existing [IFarmingGrid.cs](file:///d:/Work/Unity%20Project/BanRaiValley/Assets/Project/Scripts/Farming/Gird/IFarmingGrid.cs) and [Farming Grid.cs](file:///d:/Work/Unity%20Project/BanRaiValley/Assets/Project/Scripts/Farming/Gird/Farming%20Grid.cs).
- **Applicable Rules**:
  - [.agent/rule/architecture-guide.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/architecture-guide.md) (Interface-driven architecture, no tight coupling)
  - [.agent/rule/function-rule.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/function-rule.md) (Single responsibility, explicit parameters)

## 3. What To Do (Step-by-Step Instructions)

1. **Update `IFarmingGrid.cs`**:
   - Add the following members to the interface contract:
     ```csharp
     /// <summary>Dimensions of a single grid cell in world units.</summary>
     Vector3 CellSize { get; }

     /// <summary>Converts a world position to the nearest grid cell coordinate.</summary>
     Vector3Int WorldToCell(Vector3 worldPos);

     /// <summary>Returns the world-space center of a specific grid cell coordinate.</summary>
     Vector3 GetCellCenterWorld(Vector3Int cellPos);
     ```

2. **Implement in `Farming Grid.cs`**:
   - In `Assets/Project/Scripts/Farming/Gird/Farming Grid.cs`, implement the explicit interface members under the `#region IFarmingGrid` block:
     ```csharp
     public Vector3 CellSize => grid != null ? grid.cellSize : Vector3.one;

     public Vector3Int WorldToCell(Vector3 worldPos)
     {
         return grid != null ? grid.WorldToCell(worldPos) : Vector3Int.zero;
     }

     public Vector3 GetCellCenterWorld(Vector3Int cellPos)
     {
         return grid != null ? grid.GetCellCenterWorld(cellPos) : Vector3.zero;
     }
     ```

## 4. Verification & Testing Checklist
- [ ] Both files compile with zero warnings or errors.
- [ ] Existing callers of `IFarmingGrid` (Hoe, WateringCan, CropGrowthManager) continue functioning without regressions.
- [ ] Calling `grid.WorldToCell` and `grid.GetCellCenterWorld` returns accurate, non-zero values corresponding to the scene's Unity `Grid`.
