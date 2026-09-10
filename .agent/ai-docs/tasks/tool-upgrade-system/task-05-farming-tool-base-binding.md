# Task 05: Tool Item Binding & FarmingToolBase Integration

## 1. Task Goal
Inject `Item` configurations into spawned tool prefabs via [PlayerHandVisualizer.cs](file:///d:/Work/Unity%20Project/BanRaiValley/Assets/Project/Scripts/Inventory/Player%20Hand%20Visualizer.cs) and extend [FarmingToolBase.cs](file:///d:/Work/Unity%20Project/BanRaiValley/Assets/Project/Scripts/Farming/Tool/FarmingToolBase.cs) to implement [IToolItemReceiver](file:///d:/Work/Unity%20Project/BanRaiValley/Assets/Project/Scripts/Inventory/IToolItemReceiver.cs) with a zero-allocation multi-tile preview loop.

## 2. Task Information
- **System**: Tool Upgrade System
- **Parent Plan**: [.agent/ai-docs/plan/tool-upgrade-system-plan.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/ai-docs/plan/tool-upgrade-system-plan.md)
- **Target File(s)**:
  - `Assets/Project/Scripts/Inventory/Player Hand Visualizer.cs` [MODIFY]
  - `Assets/Project/Scripts/Farming/Tool/FarmingToolBase.cs` [MODIFY]
- **Dependencies / Prerequisites**:
  - Task 01: [IToolItemReceiver.cs](file:///d:/Work/Unity%20Project/BanRaiValley/Assets/Project/Scripts/Inventory/IToolItemReceiver.cs) & [ItemToolData.cs](file:///d:/Work/Unity%20Project/BanRaiValley/Assets/Project/Scripts/Inventory/ItemToolData.cs)
  - Task 02: [IFarmingGrid.cs](file:///d:/Work/Unity%20Project/BanRaiValley/Assets/Project/Scripts/Farming/Gird/IFarmingGrid.cs) expansion
  - Task 03: [ToolAreaCalculator.cs](file:///d:/Work/Unity%20Project/BanRaiValley/Assets/Project/Scripts/Farming/Tool/ToolAreaCalculator.cs)
  - Task 04: [EventBus.cs](file:///d:/Work/Unity%20Project/BanRaiValley/Assets/Project/Scripts/EventBus.cs) `MultiPreviewingEvent`
- **Applicable Rules**:
  - [.agent/rule/architecture-guide.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/architecture-guide.md) (Interface injection, zero GC alloc in Update loops)
  - [.agent/rule/naming-variable-rule.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/naming-variable-rule.md) (`_camelCase` private fields)
  - [.agent/rule/function-rule.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/function-rule.md)

## 3. What To Do (Step-by-Step Instructions)

1. **Update `PlayerHandVisualizer.cs`**:
   - In `SpawnItemModel(SlotData slotData)`:
     Immediately after spawning `_currentItem = LeanPool.Spawn(slotData.item.prefab, _spawnTransform);`, query for `IToolItemReceiver`:
     ```csharp
     if (_currentItem.TryGetComponent<IToolItemReceiver>(out var receiver))
     {
         receiver.BindItemData(slotData.item);
     }
     else
     {
         var childReceiver = _currentItem.GetComponentInChildren<IToolItemReceiver>();
         childReceiver?.BindItemData(slotData.item);
     }
     ```

2. **Update `FarmingToolBase.cs`**:
   - Make `FarmingToolBase` implement `IToolItemReceiver`:
     ```csharp
     public abstract class FarmingToolBase : MonoBehaviour, IToolItemReceiver
     ```
   - Add serialized fallback tool data for standalone testing:
     ```csharp
     [SerializeField] protected ItemToolData toolData;
     ```
   - Implement `BindItemData`:
     ```csharp
     public virtual void BindItemData(Item item)
     {
         if (item != null)
         {
             toolData = item.ToolData;
         }
     }
     ```
   - Add cached buffers for preview calculations (zero heap allocation in `Update`):
     ```csharp
     protected const int MAX_TOOL_CELLS = 49;
     protected readonly Vector3Int[] _cachedPatternCells = new Vector3Int[MAX_TOOL_CELLS];
     protected readonly TilePreviewData[] _cachedPreviewData = new TilePreviewData[MAX_TOOL_CELLS];
     protected int _cachedPatternCellCount;
     protected Camera sceneCamera;
     ```
   - Implement `RunMultiPreviewUpdate`:
     ```csharp
     protected void RunMultiPreviewUpdate(
         int range,
         GameObject hologramPrefab,
         PreviewState previewState,
         GridCheck check,
         float yRotation)
     {
         _ray = RayCastAtCursor();
         if (Physics.Raycast(_ray, out _hit, range))
         {
             Vector3Int centerCell = grid.WorldToCell(_hit.point);
             Vector3 lookDir = sceneCamera != null ? sceneCamera.transform.forward : Vector3.forward;
             Vector3Int cardinalDir = ToolAreaCalculator.GetCardinalFacingDirection(lookDir);

             _cachedPatternCellCount = ToolAreaCalculator.CalculatePatternCells(
                 centerCell,
                 cardinalDir,
                 toolData.patternShape,
                 toolData.patternDimension,
                 _cachedPatternCells
             );

             bool hasAnyValid = false;
             for (int i = 0; i < _cachedPatternCellCount; i++)
             {
                 Vector3 cellWorldPos = grid.GetCellCenterWorld(_cachedPatternCells[i]);
                 bool isValid = check(cellWorldPos, out _);
                 if (isValid) hasAnyValid = true;

                 _cachedPreviewData[i] = new TilePreviewData
                 {
                     Position = cellWorldPos,
                     IsValid = isValid
                 };
             }

             lastCheckWasValid = hasAnyValid;
             _lastCellWorldPos = grid.GetCellCenterWorld(centerCell);

             EventBus<StartPreviewEvent>.Raise(new StartPreviewEvent { prefabs = hologramPrefab, previewState = previewState });
             EventBus<MultiPreviewingEvent>.Raise(new MultiPreviewingEvent
             {
                 PreviewTiles = _cachedPreviewData,
                 TileCount = _cachedPatternCellCount,
                 YRotation = yRotation
             });
         }
         else
         {
             EndPreviewNow();
         }
     }
     ```

## 4. Verification & Testing Checklist
- [ ] Both files compile cleanly with zero errors.
- [ ] Spawning an item prefab with `Hoe` or `WateringCan` assigns its `toolData` from the equipped `Item`.
- [ ] Placing tool prefabs directly in a test scene without `PlayerHandVisualizer` falls back cleanly to the serialized `toolData`.
- [ ] Profiler confirms zero GC allocations per frame inside `RunMultiPreviewUpdate`.
