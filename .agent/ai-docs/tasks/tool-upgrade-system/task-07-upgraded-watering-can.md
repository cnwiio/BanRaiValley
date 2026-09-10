# Task 07: Upgraded Watering Can Square Watering

## 1. Task Goal
Update [Watering Can.cs](file:///d:/Work/Unity%20Project/BanRaiValley/Assets/Project/Scripts/Farming/Tool/Watering%20Can.cs) to support multi-cell square watering patterns (1x1, 3x3, 5x5) centered on the targeted cell, displaying a multi-cell hologram preview and watering all valid tiles simultaneously on animation impact.

## 2. Task Information
- **System**: Tool Upgrade System
- **Parent Plan**: [.agent/ai-docs/plan/tool-upgrade-system-plan.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/ai-docs/plan/tool-upgrade-system-plan.md)
- **Target File(s)**:
  - `Assets/Project/Scripts/Farming/Tool/Watering Can.cs` [MODIFY]
- **Dependencies / Prerequisites**:
  - Task 03: [ToolAreaCalculator.cs](file:///d:/Work/Unity%20Project/BanRaiValley/Assets/Project/Scripts/Farming/Tool/ToolAreaCalculator.cs)
  - Task 05: [FarmingToolBase.cs](file:///d:/Work/Unity%20Project/BanRaiValley/Assets/Project/Scripts/Farming/Tool/FarmingToolBase.cs)
- **Applicable Rules**:
  - [.agent/rule/architecture-guide.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/architecture-guide.md) (No polling in Update, event-driven)
  - [.agent/rule/naming-variable-rule.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/naming-variable-rule.md) (Descriptive names, `_camelCase` private fields)
  - [.agent/rule/function-rule.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/function-rule.md)

## 3. What To Do (Step-by-Step Instructions)

1. **Add Cached Target Cell Collection**:
   - In `Assets/Project/Scripts/Farming/Tool/Watering Can.cs`, add:
     ```csharp
     private readonly List<Vector3Int> _pendingWateringCells = new List<Vector3Int>(MAX_TOOL_CELLS);
     ```

2. **Update `Awake` / Initialization**:
   - Ensure default fallback configuration:
     ```csharp
     protected override void Awake()
     {
         base.Awake();
         if (toolData.patternDimension <= 0)
         {
             toolData = ItemToolData.DefaultWateringCan;
         }
     }
     ```

3. **Update `PrimaryAction`**:
   - Replace single-cell check with centered pattern collection:
     ```csharp
     protected override void PrimaryAction()
     {
         if (!TryGetGrid()) return;

         Vector3Int centerCell = grid.WorldToCell(_hit.point);
         int cellCount = ToolAreaCalculator.CalculatePatternCells(
             centerCell,
             Vector3Int.forward,
             toolData.patternShape,
             toolData.patternDimension,
             _cachedPatternCells
         );

         _pendingWateringCells.Clear();
         for (int i = 0; i < cellCount; i++)
         {
             Vector3 cellWorldPos = grid.GetCellCenterWorld(_cachedPatternCells[i]);
             if (grid.IsWaterable(cellWorldPos, out _))
             {
                 _pendingWateringCells.Add(_cachedPatternCells[i]);
             }
         }

         if (_pendingWateringCells.Count > 0)
         {
             StartWatering();
         }
     }
     ```

4. **Update `OnWaterinAnimationFinished`**:
   - Water all pending valid cells simultaneously:
     ```csharp
     public void OnWaterinAnimationFinished()
     {
         for (int i = 0; i < _pendingWateringCells.Count; i++)
         {
             Vector3 cellWorldPos = grid.GetCellCenterWorld(_pendingWateringCells[i]);
             if (grid.TryWatering(cellWorldPos, out var cellPos))
             {
                 EventBus<OnWateringEvent>.Raise(new OnWateringEvent
                 {
                     CellPos = cellPos,
                     Material = WateringMaterial
                 });
             }
         }

         _pendingWateringCells.Clear();
         CurrentState = WaterCanState.Farm;
     }
     ```

5. **Update `Update` Method**:
   - Call `RunMultiPreviewUpdate`:
     ```csharp
     void Update()
     {
         if (CurrentState != WaterCanState.Farm) return;
         if (!TryGetGrid()) return;

         RunMultiPreviewUpdate(WateringCanRange, hologramPrefabs, PreviewState.Watering, grid.IsWaterable, 0f);
     }
     ```

## 4. Verification & Testing Checklist
- [ ] Script compiles with zero errors.
- [ ] Basic Watering Can (dimension 1) waters 1 tile exactly as before.
- [ ] Upgraded Watering Can with `CenteredSquare` and dimension 3 waters a 3x3 square centered on the aimed tile.
- [ ] Upgraded Watering Can with `CenteredSquare` and dimension 5 waters a 5x5 square centered on the aimed tile.
- [ ] Un-tilled, obstructed, or already-watered tiles in the square are skipped safely while dry tilled soil tiles are watered.
- [ ] Hologram preview accurately displays the full NxN grid footprint in real time.
