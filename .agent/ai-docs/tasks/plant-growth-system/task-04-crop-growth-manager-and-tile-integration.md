# Task 04: Crop Growth Manager & Daily Grid Hydration Integration

## 1. Task Goal
Implement `CropGrowthManager.cs` to track all active crops across the farming grid, subscribe to `OnNewDayStartedEvent` and `OnSeasonChangedEvent`, evaluate watering and season rules to advance growth or wither crops, and coordinate with `IFarmingGrid` to reset soil hydration each morning.

## 2. Task Information
- **System**: Plant Growth System
- **Parent Plan**: [.agent/ai-docs/plan/plant-growth-system-plan.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/ai-docs/plan/plant-growth-system-plan.md)
- **Target File(s)**:
  - `Assets/Project/Scripts/Farming/Gird/IFarmingGrid.cs`
  - `Assets/Project/Scripts/Farming/Gird/TileData.cs`
  - `Assets/Project/Scripts/Farming/Gird/Farming Grid.cs`
  - `Assets/Project/Scripts/Farming/Growth/CropGrowthManager.cs`
- **Dependencies / Prerequisites**:
  - Task 01: `CropDataSO`, `ICropInstance`, `CropState`
  - Task 02: `OnCropPlantedEvent`, `OnSoilHydrationResetEvent`, `OnNewDayStartedEvent`, `OnSeasonChangedEvent`
  - Task 03: `CropInstance.cs`
- **Applicable Rules**:
  - [.agent/rule/architecture-guide.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/architecture-guide.md)
  - [.agent/rule/naming-variable-rule.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/naming-variable-rule.md)
  - [.agent/rule/function-rule.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/function-rule.md)
  - [.agent/rule/code-style-rule.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/code-style-rule.md)

## 3. What To Do (Step-by-Step Instructions)

1. **Update `ITileStore` and `TileStore` in `TileData.cs`**:
   - Add method to `ITileStore`: `int ResetDailyHydration();`
   - In `TileStore`, implement `ResetDailyHydration()`:
     - Iterate through all keys in `_tiles`, for any tile where `tile.IsWatered == true`, replace with `tile.WithWatered(false)`. Return count of modified tiles.

2. **Update `IFarmingGrid` and `FarmingGrid.cs`**:
   - In `IFarmingGrid.cs`, add:
     ```csharp
     bool IsWatered(Vector3Int cellPos);
     int ResetDailyHydration();
     ```
   - In `FarmingGrid.cs`, implement:
     ```csharp
     public bool IsWatered(Vector3Int cellPos) => _tileStore.IsWatered(cellPos);
     public int ResetDailyHydration() => _tileStore.ResetDailyHydration();
     ```

3. **Create `CropGrowthManager.cs`**:
   - Create MonoBehaviour `CropGrowthManager`:
     - Fields:
       ```csharp
       [SerializeField] private FarmingGridReference _farmingGridReference;
       [SerializeField] private GameObject _cropInstanceBasePrefab;

       private readonly Dictionary<Vector3Int, CropInstance> _activeCrops = new Dictionary<Vector3Int, CropInstance>();
       ```
     - Subscriptions in `OnEnable` / `OnDisable`:
       - `EventBus<OnPlantingEvent>.Subscribe(OnPlanting);`
       - `EventBus<OnClearPlant>.Subscribe(OnClearPlant);`
       - `EventBus<OnNewDayStartedEvent>.Subscribe(OnNewDayStarted);`
       - `EventBus<OnSeasonChangedEvent>.Subscribe(OnSeasonChanged);`
     - Handler `OnPlanting(OnPlantingEvent evt)`:
       - If `evt.CropData == null`, return or handle fallback prefab.
       - Spawn crop instance at `evt.Position` using `LeanPool` or instantiate `_cropInstanceBasePrefab` (or `evt.Prefab`).
       - Get `CropInstance` component, call `cropInstance.Initialize(evt.CellPos, evt.CropData)`.
       - Register in `_activeCrops[evt.CellPos] = cropInstance;`.
       - Raise `OnCropPlantedEvent`.
     - Handler `OnClearPlant(OnClearPlant evt)`:
       - If `_activeCrops.TryGetValue(evt.CellPos, out var crop)`:
         - Unregister `_activeCrops.Remove(evt.CellPos);`.
         - Despawn or destroy crop instance.
     - Handler `OnNewDayStarted(OnNewDayStartedEvent evt)`:
       - Step 1: For each `KeyValuePair<Vector3Int, CropInstance>` in `_activeCrops`:
         - Check season compatibility: `if (!crop.CropData.IsSeasonCompatible(evt.NewDateTime.Season))` -> `crop.SetWithered();`
         - Else if not mature and not withered:
           - Check if tile was watered: `bool wasWatered = _farmingGridReference.CurrentGrid != null && _farmingGridReference.CurrentGrid.IsWatered(cellPos);`
           - Call `crop.AdvanceGrowthDay(wasWatered);`
       - Step 2: Reset soil hydration for the new day:
         - `if (_farmingGridReference.CurrentGrid != null)`:
           - `int resetCount = _farmingGridReference.CurrentGrid.ResetDailyHydration();`
           - `EventBus<OnSoilHydrationResetEvent>.Raise(new OnSoilHydrationResetEvent { ResetTilesCount = resetCount });`
     - Handler `OnSeasonChanged(OnSeasonChangedEvent evt)`:
       - Check all active crops against `evt.NewSeason`; call `crop.SetWithered()` on any incompatible active crops.

## 4. Verification & Testing Checklist
- [ ] No polling in `Update`.
- [ ] All event handlers cleanly subscribe in `OnEnable` and unsubscribe in `OnDisable`.
- [ ] Soil hydration is checked *before* daily hydration is reset on morning tick.
- [ ] Incompatible crops wither upon `OnSeasonChangedEvent` and `OnNewDayStartedEvent`.
