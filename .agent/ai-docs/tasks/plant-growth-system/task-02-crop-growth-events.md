# Task 02: Plant Growth Event Signatures & EventBus Integration

## 1. Task Goal
Add and update strongly-typed event structs in `EventBus.cs` for crop planting, stage progression, withering, harvesting, and soil hydration resetting.

## 2. Task Information
- **System**: Plant Growth System
- **Parent Plan**: [.agent/ai-docs/plan/plant-growth-system-plan.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/ai-docs/plan/plant-growth-system-plan.md)
- **Target File(s)**:
  - `Assets/Project/Scripts/EventBus.cs`
- **Dependencies / Prerequisites**:
  - Task 01: `CropDataSO`, `CropStageData`, `CropState`, `ICropInstance`
- **Applicable Rules**:
  - [.agent/rule/architecture-guide.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/architecture-guide.md)
  - [.agent/rule/naming-variable-rule.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/naming-variable-rule.md)
  - [.agent/rule/code-style-rule.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/code-style-rule.md)

## 3. What To Do (Step-by-Step Instructions)

1. **Update `OnPlantingEvent`**:
   - Enhance `OnPlantingEvent` in `EventBus.cs` to carry the `CropDataSO` reference while keeping backwards compatibility with `GameObject Prefab`:
     ```csharp
     public struct OnPlantingEvent : IEvent
     {
         public CropDataSO CropData;
         public GameObject Prefab;
         public Vector3 Position;
         public Vector3Int CellPos;
     }
     ```

2. **Add New Plant Growth Event Structs to `EventBus.cs` under a dedicated `#region Plant Growth Events`**:
   ```csharp
   #region Plant Growth Events

   /// <summary>
   /// Raised when a new crop instance is spawned and registered on a farm tile.
   /// </summary>
   public struct OnCropPlantedEvent : IEvent
   {
       public GameObject CropInstance;
       public Vector3Int CellPos;
       public CropDataSO CropData;
   }

   /// <summary>
   /// Raised when a crop advances its visual stage or reaches maturity on morning tick.
   /// </summary>
   public struct OnCropStageChangedEvent : IEvent
   {
       public GameObject CropInstance;
       public Vector3Int CellPos;
       public int PreviousStageIndex;
       public int NewStageIndex;
       public bool IsMature;
   }

   /// <summary>
   /// Raised when a crop withers due to out-of-season rollover or neglect.
   /// </summary>
   public struct OnCropWitheredEvent : IEvent
   {
       public GameObject CropInstance;
       public Vector3Int CellPos;
       public CropDataSO CropData;
   }

   /// <summary>
   /// Raised when the player triggers harvest interaction on a mature crop.
   /// </summary>
   public struct OnCropHarvestRequestedEvent : IEvent
   {
       public GameObject CropInstance;
       public Vector3Int CellPos;
       public GameObject Interactor;
   }

   /// <summary>
   /// Raised after daily crop growth evaluation when soil hydration resets to dry.
   /// </summary>
   public struct OnSoilHydrationResetEvent : IEvent
   {
       public int ResetTilesCount;
   }

   #endregion
   ```

## 4. Verification & Testing Checklist
- [ ] `EventBus.cs` compiles with zero warnings/errors.
- [ ] All new structs implement `IEvent`.
- [ ] All event fields use PascalCase names with descriptive intent.
