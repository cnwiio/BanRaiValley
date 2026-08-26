# Task 03: Crop Instance & Multi-Stage Visual Swapper

## 1. Task Goal
Implement `CropInstance.cs` as the runtime representation of a growing crop on a specific tile. It manages stage advancement, visual swapping (activating/instantiating stage models without despawning root), withering, and implements `IInteractable` to handle first-person raycast harvest prompts.

## 2. Task Information
- **System**: Plant Growth System
- **Parent Plan**: [.agent/ai-docs/plan/plant-growth-system-plan.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/ai-docs/plan/plant-growth-system-plan.md)
- **Target File(s)**:
  - `Assets/Project/Scripts/Farming/Growth/CropInstance.cs`
- **Dependencies / Prerequisites**:
  - Task 01: `CropDataSO`, `CropStageData`, `CropState`, `ICropInstance`
  - Task 02: `OnCropStageChangedEvent`, `OnCropWitheredEvent`, `OnCropHarvestRequestedEvent`
  - `Assets/Project/Scripts/Time/IInteractable.cs`
  - `Assets/Project/Scripts/AI/PlantAI/AwakenedCropHarvestTrigger.cs`
- **Applicable Rules**:
  - [.agent/rule/architecture-guide.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/architecture-guide.md)
  - [.agent/rule/naming-variable-rule.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/naming-variable-rule.md)
  - [.agent/rule/function-rule.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/function-rule.md)
  - [.agent/rule/code-style-rule.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/code-style-rule.md)

## 3. What To Do (Step-by-Step Instructions)

1. **Create `CropInstance.cs` implementing `MonoBehaviour`, `ICropInstance`, and `IInteractable`**:
   - Fields:
     ```csharp
     [Header("Transforms & Containers")]
     [SerializeField] private Transform _visualContainer;
     [SerializeField] private AwakenedCropHarvestTrigger _awakenedHarvestTrigger;

     private Vector3Int _cellPos;
     private CropDataSO _cropData;
     private CropState _currentState = CropState.Growing;
     private int _currentStageIndex = 0;
     private int _daysInCurrentStage = 0;
     private GameObject _activeVisualInstance;
     ```

2. **Implement `ICropInstance` Properties**:
   - `public Vector3Int CellPos => _cellPos;`
   - `public CropDataSO CropData => _cropData;`
   - `public CropState CurrentState => _currentState;`
   - `public int CurrentStageIndex => _currentStageIndex;`
   - `public int DaysInCurrentStage => _daysInCurrentStage;`
   - `public bool IsMature => _currentState == CropState.Mature;`
   - `public bool IsWithered => _currentState == CropState.Withered;`

3. **Implement Lifecycle & Initialization**:
   - `public void Initialize(Vector3Int cellPos, CropDataSO cropData)`:
     - Store `_cellPos = cellPos` and `_cropData = cropData`.
     - Reset `_currentState = CropState.Growing`, `_currentStageIndex = 0`, `_daysInCurrentStage = 0`.
     - If `_awakenedHarvestTrigger != null`, call `_awakenedHarvestTrigger.Initialize(cellPos)`.
     - Update visuals to Stage 0 by calling `UpdateVisualForCurrentStage()`.

4. **Implement Stage Advancement & Withering**:
   - `public void AdvanceGrowthDay(bool wasWatered)`:
     - If not `CropState.Growing` or not watered, return.
     - Increment `_daysInCurrentStage++`.
     - Check if `_daysInCurrentStage >= _cropData.Stages[_currentStageIndex].DaysRequired`.
     - If yes, advance stage:
       - If `_currentStageIndex < _cropData.FinalStageIndex`:
         - `int previousIndex = _currentStageIndex;`
         - `_currentStageIndex++;`
         - `_daysInCurrentStage = 0;`
         - If `_currentStageIndex == _cropData.FinalStageIndex`:
           - `_currentState = CropState.Mature;`
         - `UpdateVisualForCurrentStage();`
         - Raise `OnCropStageChangedEvent`.
   - `public void SetWithered()`:
     - `_currentState = CropState.Withered;`
     - Spawn/Swap to `_cropData.WitheredPrefab` in `_visualContainer`.
     - Raise `OnCropWitheredEvent`.
   - `public void ResetToRegrowth()`:
     - If `_cropData.IsRegrowable`:
       - `_currentState = CropState.Growing;`
       - `_currentStageIndex = _cropData.RegrowStageIndex;`
       - `_daysInCurrentStage = 0;`
       - `UpdateVisualForCurrentStage();`
       - Raise `OnCropStageChangedEvent`.

5. **Implement `IInteractable`**:
   - `public string InteractionLabel`:
     - If `_currentState == CropState.Mature`: return `"Harvest " + _cropData.CropName;`
     - If `_currentState == CropState.Withered`: return `"Clear Withered Crop";`
     - Otherwise return `string.Empty;`
   - `public bool CanInteract(GameObject interactor)`:
     - Return `_currentState == CropState.Mature || _currentState == CropState.Withered;`
   - `public void Interact(GameObject interactor)`:
     - If `_currentState == CropState.Mature`:
       - Raise `OnCropHarvestRequestedEvent`.
       - If `_awakenedHarvestTrigger != null`:
         - Call `_awakenedHarvestTrigger.TriggerAwakening();`
       - Else:
         - Raise `OnClearPlant { CellPos = _cellPos }`.
     - If `_currentState == CropState.Withered`:
       - Raise `OnClearPlant { CellPos = _cellPos }`.

6. **Implement Visual Helper (`UpdateVisualForCurrentStage`)**:
   - Destroy or return previous `_activeVisualInstance` to pool.
   - If current stage has valid `StageVisualPrefab`, instantiate under `_visualContainer` (or transform) and reset local position/rotation.

## 4. Verification & Testing Checklist
- [ ] No polling in `Update`.
- [ ] Implements `ICropInstance` and `IInteractable` completely.
- [ ] Visual swapping correctly destroys/pools old stage visuals before instantiating next stage.
- [ ] All private fields follow `_camelCase` with modal booleans.
