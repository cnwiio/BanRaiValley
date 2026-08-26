# Task 05: Seed Bag & Item Data Integration

## 1. Task Goal
Integrate `CropDataSO` into `Item.cs` for `ItemType.Seed` items, and update `Seed Bag.cs` to pass the equipped seed's `CropDataSO` into `OnPlantingEvent` to dynamically drive crop instantiation and growth.

## 2. Task Information
- **System**: Plant Growth System
- **Parent Plan**: [.agent/ai-docs/plan/plant-growth-system-plan.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/ai-docs/plan/plant-growth-system-plan.md)
- **Target File(s)**:
  - `Assets/Project/Scripts/Inventory/Item.cs`
  - `Assets/Project/Scripts/Farming/Tool/Seed Bag.cs`
- **Dependencies / Prerequisites**:
  - Task 01: `CropDataSO`
  - Task 02: `OnPlantingEvent` with `CropDataSO` field
  - Task 04: `CropGrowthManager`
- **Applicable Rules**:
  - [.agent/rule/architecture-guide.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/architecture-guide.md)
  - [.agent/rule/naming-variable-rule.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/naming-variable-rule.md)
  - [.agent/rule/function-rule.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/function-rule.md)
  - [.agent/rule/code-style-rule.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/code-style-rule.md)

## 3. What To Do (Step-by-Step Instructions)

1. **Update `Item.cs`**:
   - Add a serialized field and public property for `CropDataSO`:
     ```csharp
     [Header("Farming / Seed Data")]
     [SerializeField] private CropDataSO _cropData;

     /// <summary>Crop growth configuration if this item is a seed.</summary>
     public CropDataSO CropData => _cropData;
     ```

2. **Update `Seed Bag.cs`**:
   - Add field `[SerializeField] private CropDataSO defaultCropData;` (fallback for editor testing).
   - Add method or field `private CropDataSO _currentCropData;`
   - In `OnEnable`, subscribe to `EventBus<OnHotbarChangeEvent>` to detect the equipped seed item:
     ```csharp
     private void OnHotbarChange(OnHotbarChangeEvent evt)
     {
         if (evt.slotData != null && evt.slotData.Item != null && evt.slotData.Item.type == ItemType.Seed)
         {
             _currentCropData = evt.slotData.Item.CropData;
         }
         else
         {
             _currentCropData = defaultCropData;
         }
     }
     ```
   - In `OnPlantingAnimationFinished()`:
     - When raising `OnPlantingEvent`, populate `CropData = _currentCropData ?? defaultCropData`:
       ```csharp
       EventBus<OnPlantingEvent>.Raise(new OnPlantingEvent
       {
           CropData = _currentCropData ?? defaultCropData,
           Prefab = plantPrefab,
           Position = _plantingPos,
           CellPos = cellPos
       });
       ```

## 4. Verification & Testing Checklist
- [ ] `Item.cs` and `Seed Bag.cs` compile cleanly with zero errors.
- [ ] `Seed Bag` dynamically updates its active `CropDataSO` when hotbar slot changes.
- [ ] `OnPlantingEvent` reliably carries valid `CropDataSO` into `CropGrowthManager`.
