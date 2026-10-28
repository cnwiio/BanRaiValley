# Task 03: Continuous Planting & Sowing Animation Loop in SeedBag

## 1. Task Goal
Enable continuous planting while holding Left Mouse Button with `Seed Bag` equipped, looping the sowing animation as the player sweeps the cursor across valid tilled tiles, and cleanly returning to Idle when released or when seeds run out.

## 2. Task Information
- **System**: Farming & Player Interaction
- **Parent Plan**: [continuous-planting-harvesting-plan.md](../../plan/continuous-planting-harvesting-plan.md)
- **Target File(s)**:
  - `Assets/Project/Scripts/Farming/Tool/Seed Bag.cs`
  - `Assets/Project/Animation/Seed Bag/Planting.anim`
  - `Assets/Project/Animation/Seed Bag/Seed bag.controller`
- **Dependencies / Prerequisites**:
  - Task 01 must be completed first (`OnPrimaryActionStartedEvent`, `OnPrimaryActionCanceledEvent`).
- **Applicable Rules**:
  - `architecture-guide.md`: Event-driven state transitions, decoupled inventory integration via `OnPlantingEvent`.
  - `naming-variable-rule.md`: Modal boolean prefixes (`_isPrimaryHeld`, `_isPlantingActive`), `_camelCase` naming.
  - `function-rule.md`: Command vs Query separation.

## 3. What To Do (Step-by-Step Instructions)
1. **Animation Assets**:
   - In `Planting.anim`: Set `m_LoopTime: 1`.
   - In `Seed bag.controller`:
     - Add animator parameter: `IsPlanting` (Type: `Bool`, Default: `false`).
     - Transition `Idle -> Planting`: Condition `IsPlanting == true` (HasExitTime: false, Duration: 0.1s).
     - Transition `Planting -> Idle`: Condition `IsPlanting == false` (HasExitTime: true, ExitTime: 0.9s, Duration: 0.15s).
     - Keep `Sowing` trigger for single-click compatibility.
2. **`SeedBag.cs` Implementation**:
   - Subscribe to `OnPrimaryActionStartedEvent` and `OnPrimaryActionCanceledEvent` in `OnEnable`/`OnDisable`.
   - Track `_isPrimaryHeld`, `_plantingTimer`, and `_lastPlantedCellPos`.
   - On Primary Started:
     - Set `_isPrimaryHeld = true;`
     - Attempt initial plant immediately if aiming at a valid plantable tile.
     - Set animator `IsPlanting = true;`
   - In `Update()`:
     - Allow preview update to continue running so holograms update while moving.
     - When `_isPrimaryHeld` is true:
       - Update `_plantingTimer -= Time.deltaTime;`
       - If `_plantingTimer <= 0f` and `grid.IsPlantable(_hit.point, out var cellWorldPos)`:
         - Execute planting logic: call `grid.TryPlanting(cellWorldPos, out var cellPos)`.
         - Raise `OnPlantingEvent` (which informs `PlantManager` and `PlayerInventoryManager`).
         - Reset `_plantingTimer = 0.18f;`
         - Update preview tile data.
   - On Primary Canceled / OnDisable / Out of Seeds:
     - Set `_isPrimaryHeld = false;`
     - Set animator `IsPlanting = false;`
     - Return `CurrentState = SeedBagState.Farm;`

## 4. Verification & Testing Checklist
- [ ] Script compiles with zero warnings/errors in Unity 6.3.
- [ ] Single click still plants one seed.
- [ ] Holding LMB continuously loops the sowing animation and plants across multiple dirt plots.
- [ ] Releasing LMB smoothly returns the seed bag to Idle.
- [ ] Running out of seeds stops planting and unequips seed bag gracefully.
