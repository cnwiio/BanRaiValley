# Task 06: Harvest Interaction, Awakened Monster Spawning & Withering Cleanup

## 1. Task Goal
Integrate mature crop raycast interaction (`IInteractable`), `AwakenedCropHarvestTrigger`, and tool/interaction cleanup of withered crops, ensuring seamless transition between mature farming visuals and awakened monster combat encounters.

## 2. Task Information
- **System**: Plant Growth System
- **Parent Plan**: [.agent/ai-docs/plan/plant-growth-system-plan.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/ai-docs/plan/plant-growth-system-plan.md)
- **Target File(s)**:
  - `Assets/Project/Scripts/AI/PlantAI/AwakenedCropHarvestTrigger.cs`
  - `Assets/Project/Scripts/Farming/Growth/CropInstance.cs`
- **Dependencies / Prerequisites**:
  - Task 03: `CropInstance.cs`
  - Task 04: `CropGrowthManager.cs`
  - `Assets/Project/Scripts/AI/PlantAI/PlantBrain.cs`
- **Applicable Rules**:
  - [.agent/rule/architecture-guide.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/architecture-guide.md)
  - [.agent/rule/naming-variable-rule.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/naming-variable-rule.md)
  - [.agent/rule/function-rule.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/function-rule.md)
  - [.agent/rule/code-style-rule.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/code-style-rule.md)

## 3. What To Do (Step-by-Step Instructions)

1. **Update `AwakenedCropHarvestTrigger.cs`**:
   - Add method to set monster prefab dynamically from `CropDataSO`:
     ```csharp
     public void SetMonsterPrefab(GameObject monsterPrefab)
     {
         if (monsterPrefab != null)
         {
             _awakenedMonsterPrefab = monsterPrefab;
         }
     }
     ```
   - Ensure `TriggerAwakening()` checks if the crop instance is regrowable:
     - If not regrowable, raise `OnClearPlant { CellPos = _cellPos }` so the tile clears.
     - If regrowable, do NOT raise `OnClearPlant`; instead notify `CropInstance.ResetToRegrowth()`.
     - Spawns the monster from `LeanPool` and calls `brain.Awaken()`.
     - Raises `OnPlantAwakenedEvent`.

2. **Refine Interaction in `CropInstance.cs`**:
   - When mature: `CanInteract(GameObject interactor)` returns true, `InteractionLabel` returns `"[E] Awaken & Harvest " + _cropData.CropName`.
   - On `Interact()`:
     - If `_awakenedHarvestTrigger != null`:
       - `_awakenedHarvestTrigger.SetMonsterPrefab(_cropData.AwakenedMonsterPrefab);`
       - `_awakenedHarvestTrigger.TriggerAwakening();`
       - If `_cropData.IsRegrowable`:
         - `ResetToRegrowth();`
     - Else:
       - Raise `OnClearPlant { CellPos = _cellPos }`.
   - When withered: `InteractionLabel` returns `"[E] Clear Withered Crop"`.
   - On `Interact()` when withered:
     - Raise `OnClearPlant { CellPos = _cellPos }`.

3. **Tool Clearing for Withered Crops**:
   - Ensure hitting a withered crop tile with a Scythe / Hoe triggers `OnClearPlant` on the event bus to clear the withered crop instance and untill/restore the tile.

## 4. Verification & Testing Checklist
- [ ] Mature crop presents interactive prompt to the player raycast.
- [ ] Triggering harvest successfully spawns the awakened monster with `PlantBrain.Awaken()`.
- [ ] Regrowable crops revert to `RegrowStageIndex` while non-regrowable crops are cleared.
- [ ] Withered crops can be cleared via interaction or tool action.
