# Task 06: Upgraded Hoe Straight-Line Tilling

## 1. Task Goal
Update [Hoe.cs](file:///d:/Work/Unity%20Project/BanRaiValley/Assets/Project/Scripts/Farming/Tool/Hoe.cs) to support multi-cell straight-line tilling in the player's nearest cardinal facing direction, displaying a multi-cell hologram preview and tilling all valid tiles simultaneously on animation impact.

## 2. Task Information
- **System**: Tool Upgrade System
- **Parent Plan**: [.agent/ai-docs/plan/tool-upgrade-system-plan.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/ai-docs/plan/tool-upgrade-system-plan.md)
- **Target File(s)**:
  - `Assets/Project/Scripts/Farming/Tool/Hoe.cs` [MODIFY]
- **Dependencies / Prerequisites**:
  - Task 03: [ToolAreaCalculator.cs](file:///d:/Work/Unity%20Project/BanRaiValley/Assets/Project/Scripts/Farming/Tool/ToolAreaCalculator.cs)
  - Task 05: [FarmingToolBase.cs](file:///d:/Work/Unity%20Project/BanRaiValley/Assets/Project/Scripts/Farming/Tool/FarmingToolBase.cs)
- **Applicable Rules**:
  - [.agent/rule/architecture-guide.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/architecture-guide.md) (No polling in Update, event-driven, zero leaks)
  - [.agent/rule/naming-variable-rule.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/naming-variable-rule.md) (Descriptive names, `_camelCase` private fields)
  - [.agent/rule/function-rule.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/function-rule.md)

## 3. What To Do (Step-by-Step Instructions)

1. **Add Cached Target Cell Collection**:
   - In `Assets/Project/Scripts/Farming/Tool/Hoe.cs`, add:
     ```csharp
     private readonly List<Vector3Int> _pendingTillingCells = new List<Vector3Int>(MAX_TOOL_CELLS);
     ```

2. **Update `Awake` / Initialization**:
   - Ensure default fallback configuration if unassigned:
     ```csharp
     if (toolData.patternDimension <= 0)
     {
         toolData = ItemToolData.DefaultHoe;
     }
     ```

3. **Update `PrimaryAction` in `HoeState.Farming`**:
   - Replace single-cell tilling check with pattern gathering:
     ```csharp
     case HoeState.Farming:
         if (!TryGetGrid()) return;

         Vector3Int centerCell = grid.WorldToCell(_hit.point);
         Vector3 lookDir = sceneCamera != null ? sceneCamera.transform.forward : Vector3.forward;
         Vector3Int cardinalDir = ToolAreaCalculator.GetCardinalFacingDirection(lookDir);

         int cellCount = ToolAreaCalculator.CalculatePatternCells(
             centerCell,
             cardinalDir,
             toolData.patternShape,
             toolData.patternDimension,
             _cachedPatternCells
         );

         _pendingTillingCells.Clear();
         for (int i = 0; i < cellCount; i++)
         {
             Vector3 cellWorldPos = grid.GetCellCenterWorld(_cachedPatternCells[i]);
             if (grid.IsValidForTilling(cellWorldPos, out _))
             {
                 _pendingTillingCells.Add(_cachedPatternCells[i]);
             }
         }

         if (_pendingTillingCells.Count > 0)
         {
             StartTilling();
         }
         break;
     ```

4. **Update `OnTillingAnimationFinish`**:
   - Till all pending valid cells simultaneously:
     ```csharp
     public void OnTillingAnimationFinish()
     {
         for (int i = 0; i < _pendingTillingCells.Count; i++)
         {
             Vector3 cellWorldPos = grid.GetCellCenterWorld(_pendingTillingCells[i]);
             if (grid.TryTill(cellWorldPos, out var cellPos))
             {
                 EventBus<OnTillingImpactEvent>.Raise(new OnTillingImpactEvent
                 {
                     prefabs = dirtPrefabs,
                     Position = cellWorldPos,
                     YRotation = currentYRotate,
                     CellPos = cellPos
                 });
             }
         }

         _pendingTillingCells.Clear();
         CurrentState = HoeState.Farming;
     }
     ```

5. **Update `Update` Method**:
   - Use `RunMultiPreviewUpdate` for `HoeState.Farming`:
     ```csharp
     void Update()
     {
         if (!TryGetGrid()) return;

         if (CurrentState == HoeState.Farming)
         {
             RunMultiPreviewUpdate(HoeRange, dirtHologramPrefabs, PreviewState.Build, grid.IsValidForTilling, currentYRotate);
         }
         else if (CurrentState == HoeState.Deleting)
         {
             RunPreviewUpdate(HoeRange, deleteHologramPrefabs, PreviewState.Delete, grid.IsTilled, currentYRotate);
         }
     }
     ```

## 4. Verification & Testing Checklist
- [ ] Script compiles with zero errors.
- [ ] Basic Hoe (dimension 1) tills 1 tile exactly as before.
- [ ] Upgraded Hoe with `ForwardLine` and dimension 3 tills a 3-tile line starting at target cell extending forward in the player's facing direction.
- [ ] Upgraded Hoe with `ForwardLine` and dimension 5 tills a 5-tile line.
- [ ] Obstacles or already-tilled tiles within the line are skipped while remaining valid tiles are tilled successfully.
- [ ] Previews accurately show multiple green/red hologram tiles for the entire line.
