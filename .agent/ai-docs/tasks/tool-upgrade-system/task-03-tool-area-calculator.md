# Task 03: Pure Service: Tool Area Calculator

## 1. Task Goal
Implement a pure, testable, static service class `ToolAreaCalculator` to resolve grid cell footprint patterns (straight lines in cardinal facing direction and centered NxN squares) with zero runtime GC allocation.

## 2. Task Information
- **System**: Tool Upgrade System
- **Parent Plan**: [.agent/ai-docs/plan/tool-upgrade-system-plan.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/ai-docs/plan/tool-upgrade-system-plan.md)
- **Target File(s)**:
  - `Assets/Project/Scripts/Farming/Tool/ToolAreaCalculator.cs` [NEW]
- **Dependencies / Prerequisites**:
  - Task 01: [ItemToolData.cs](file:///d:/Work/Unity%20Project/BanRaiValley/Assets/Project/Scripts/Inventory/ItemToolData.cs)
- **Applicable Rules**:
  - [.agent/rule/architecture-guide.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/architecture-guide.md) (Pure services, zero GC in hot loops)
  - [.agent/rule/naming-variable-rule.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/naming-variable-rule.md) (Domain descriptive names, no abbreviations)
  - [.agent/rule/function-rule.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/function-rule.md) (Pure functions, explicit parameters)

## 3. What To Do (Step-by-Step Instructions)

1. **Create `ToolAreaCalculator.cs`**:
   - Location: `Assets/Project/Scripts/Farming/Tool/ToolAreaCalculator.cs`
   - Mark as `public static class ToolAreaCalculator`.

2. **Implement `GetCardinalFacingDirection`**:
   ```csharp
   /// <summary>
   /// Snaps a 3D view direction vector onto the horizontal XZ plane and returns the closest cardinal grid step.
   /// </summary>
   /// <param name="lookDirection">The forward vector (typically from camera or player transform).</param>
   /// <returns>A unit Vector3Int in one of the 4 cardinal grid directions: +Z (North), -Z (South), +X (East), -X (West).</returns>
   public static Vector3Int GetCardinalFacingDirection(Vector3 lookDirection)
   {
       Vector3 flatDirection = new Vector3(lookDirection.x, 0f, lookDirection.z);
       if (flatDirection.sqrMagnitude < 0.001f)
       {
           return Vector3Int.forward;
       }

       flatDirection.Normalize();

       if (Mathf.Abs(flatDirection.x) > Mathf.Abs(flatDirection.z))
       {
           return flatDirection.x > 0f ? Vector3Int.right : Vector3Int.left;
       }
       else
       {
           return flatDirection.z > 0f ? Vector3Int.forward : Vector3Int.back;
       }
   }
   ```

3. **Implement `CalculatePatternCells` (Zero-GC Buffer Fill)**:
   ```csharp
   /// <summary>
   /// Fills a pre-allocated buffer with cell coordinates based on the tool's pattern shape and dimension.
   /// </summary>
   /// <param name="centerCell">The primary targeted grid cell.</param>
   /// <param name="cardinalDirection">The cardinal step direction for directional patterns.</param>
   /// <param name="pattern">The pattern shape (Single, ForwardLine, CenteredSquare).</param>
   /// <param name="dimension">The pattern dimension (e.g. 1, 3, 5).</param>
   /// <param name="resultBuffer">Pre-allocated array to receive cell positions.</param>
   /// <returns>The number of valid cells populated into resultBuffer.</returns>
   public static int CalculatePatternCells(
       Vector3Int centerCell,
       Vector3Int cardinalDirection,
       ToolAreaPattern pattern,
       int dimension,
       Vector3Int[] resultBuffer)
   {
       if (resultBuffer == null || resultBuffer.Length == 0) return 0;

       if (dimension <= 1 || pattern == ToolAreaPattern.Single)
       {
           resultBuffer[0] = centerCell;
           return 1;
       }

       switch (pattern)
       {
           case ToolAreaPattern.ForwardLine:
           {
               int lineLength = Mathf.Min(dimension, resultBuffer.Length);
               for (int i = 0; i < lineLength; i++)
               {
                   resultBuffer[i] = centerCell + cardinalDirection * i;
               }
               return lineLength;
           }

           case ToolAreaPattern.CenteredSquare:
           {
               int radius = dimension / 2;
               int populatedCount = 0;

               for (int dz = -radius; dz <= radius; dz++)
               {
                   for (int dx = -radius; dx <= radius; dx++)
                   {
                       if (populatedCount >= resultBuffer.Length) break;
                       resultBuffer[populatedCount++] = new Vector3Int(
                           centerCell.x + dx,
                           centerCell.y,
                           centerCell.z + dz
                       );
                   }
               }
               return populatedCount;
           }

           default:
               resultBuffer[0] = centerCell;
               return 1;
       }
   }
   ```

4. **Implement Helper `GetMaxTileCount`**:
   ```csharp
   public static int GetMaxTileCount(ToolAreaPattern pattern, int dimension)
   {
       if (dimension <= 1 || pattern == ToolAreaPattern.Single) return 1;
       return pattern == ToolAreaPattern.CenteredSquare ? dimension * dimension : dimension;
   }
   ```

## 4. Verification & Testing Checklist
- [ ] Script compiles cleanly without errors.
- [ ] Line pattern starting at `(0, 0, 0)` facing North (`0, 0, 1`) with dimension 3 fills `(0, 0, 0)`, `(0, 0, 1)`, `(0, 0, 2)`.
- [ ] Square pattern centered at `(0, 0, 0)` with dimension 3 fills a 3x3 footprint from `(-1, 0, -1)` to `(1, 0, 1)`.
- [ ] Populating into `resultBuffer` causes zero heap garbage allocations.
