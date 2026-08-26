# Task 01: Core Crop Data Models, Enums & ScriptableObjects

## 1. Task Goal
Implement the core data structures, enums, interfaces, and `CropDataSO` ScriptableObject defining crop lifecycle states, seasonal compatibility, growth stage progression data, withering, regrowth configurations, and awakened monster prefab references.

## 2. Task Information
- **System**: Plant Growth System
- **Parent Plan**: [.agent/ai-docs/plan/plant-growth-system-plan.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/ai-docs/plan/plant-growth-system-plan.md)
- **Target File(s)**:
  - `Assets/Project/Scripts/Farming/Growth/CropState.cs`
  - `Assets/Project/Scripts/Farming/Growth/CropStageData.cs`
  - `Assets/Project/Scripts/Farming/Growth/CropDataSO.cs`
  - `Assets/Project/Scripts/Farming/Growth/ICropInstance.cs`
- **Dependencies / Prerequisites**:
  - `Assets/Project/Scripts/Time/Data/Season.cs` (Existing Season enum)
  - `Assets/Project/Scripts/Inventory/Item.cs` (Existing Item class)
- **Applicable Rules**:
  - [.agent/rule/architecture-guide.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/architecture-guide.md)
  - [.agent/rule/naming-variable-rule.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/naming-variable-rule.md)
  - [.agent/rule/function-rule.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/function-rule.md)
  - [.agent/rule/code-style-rule.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/code-style-rule.md)

## 3. What To Do (Step-by-Step Instructions)

1. **Create `CropState.cs`**:
   - Create enum `CropState` with values:
     - `Growing`: In the middle of growing through stages.
     - `Mature`: Fully grown; ready for player interaction to awaken or harvest.
     - `Withered`: Out-of-season or neglected; clearable.
     - `Harvested`: Currently awakened or harvested.

2. **Create `CropStageData.cs`**:
   - Create `[System.Serializable] public struct CropStageData`:
     - `public int StageIndex;`
     - `public int DaysRequired;` (Number of watered days required in this stage)
     - `public GameObject StageVisualPrefab;` (Visual model prefab for this stage)

3. **Create `ICropInstance.cs`**:
   - Create public interface `ICropInstance`:
     ```csharp
     public interface ICropInstance
     {
         Vector3Int CellPos { get; }
         CropDataSO CropData { get; }
         CropState CurrentState { get; }
         int CurrentStageIndex { get; }
         int DaysInCurrentStage { get; }
         bool IsMature { get; }
         bool IsWithered { get; }

         void Initialize(Vector3Int cellPos, CropDataSO cropData);
         void AdvanceGrowthDay(bool wasWatered);
         void SetWithered();
         void ResetToRegrowth();
     }
     ```

4. **Create `CropDataSO.cs`**:
   - Create `[CreateAssetMenu(fileName = "CropData_", menuName = "BanRaiValley/Farming/Crop Data")] public class CropDataSO : ScriptableObject`:
     - `public string CropId;`
     - `public string CropName;`
     - `[TextArea(2, 4)] public string Description;`
     - `public List<Season> CompatibleSeasons = new List<Season>();`
     - `public List<CropStageData> Stages = new List<CropStageData>();`
     - `public GameObject WitheredPrefab;`
     - `public GameObject AwakenedMonsterPrefab;`
     - `public bool IsRegrowable;`
     - `public int RegrowStageIndex;`
     - `public int RegrowDays;`
     - `public Item HarvestItem;`
     - `public Item SeedItem;`
     - Properties and helpers:
       - `public int FinalStageIndex => Stages.Count > 0 ? Stages.Count - 1 : 0;`
       - `public bool IsSeasonCompatible(Season season) => CompatibleSeasons != null && CompatibleSeasons.Contains(season);`

## 4. Verification & Testing Checklist
- [ ] Scripts compile with zero warnings/errors in Unity 6.3.
- [ ] All types are in their own dedicated files with correct casing and namespace/comments.
- [ ] `CropDataSO` can be created via the Unity Asset Menu (`BanRaiValley/Farming/Crop Data`).
- [ ] All boolean properties use modal verbs (`IsMature`, `IsWithered`, `IsRegrowable`).
