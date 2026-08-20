# Task 04: Tool Context-Aware Attack Integration

## 1. Task Goal
Integrate context-aware attack triggering into tools like the Hoe (`Hoe.cs`) and establish clean delegation so tools attack when in `Idle` state and perform their farming actions when in tool-specific active modes (`Farming`, `Deleting`).

## 2. Task Information
- **System**: Player Attack System
- **Parent Plan**: [.agent/ai-docs/plan/player-attack-plan.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/ai-docs/plan/player-attack-plan.md)
- **Target File(s)**:
  - `Assets/Project/Scripts/Farming/Tool/Hoe.cs` (MODIFY)
  - `Assets/Project/Scripts/Farming/Tool/FarmingToolBase.cs` (MODIFY)
- **Dependencies / Prerequisites**:
  - Task 01, Task 03
- **Applicable Rules**:
  - [.agent/rule/architecture-guide.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/architecture-guide.md)
  - [.agent/rule/naming-variable-rule.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/naming-variable-rule.md)
  - [.agent/rule/function-rule.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/function-rule.md)

## 3. What To Do (Step-by-Step Instructions)

1. **Context-Aware Primary Action Handling in `Hoe.cs`**:
   - In `Assets/Project/Scripts/Farming/Tool/Hoe.cs`:
     - Inspect `PrimaryAction()`:
       - If `CurrentState == HoeState.Idle`:
         - Raise `EventBus<OnPlayerRequestAttackEvent>.Raise(new OnPlayerRequestAttackEvent { });`
       - If `CurrentState == HoeState.Farming`:
         - If `!TryGetGrid()` return;
         - If `grid.IsValidForTilling(_hit.point, out var cellWorldPos)`:
           - `_dirtPos = cellWorldPos;`
           - `StartTilling();`
       - If `CurrentState == HoeState.Deleting`:
         - If `!TryGetGrid()` return;
         - Perform plant/tile deletion.

2. **Ensure Clean Input Separation in `PlayerCombatController`**:
   - In `PlayerCombatController`:
     - If the player is holding an active `FarmingToolBase` (such as `Hoe`), the tool script subscribes to `OnPrimaryActionEvent` and intercepts the click.
     - When `CurrentState == HoeState.Idle`, `Hoe` forwards the attack intent via `OnPlayerRequestAttackEvent`.
     - When no tool component is active (e.g. Bare Hand, Weapon, or non-tool item), `PlayerCombatController` directly handles `OnPrimaryActionEvent`.

3. **Tool Animation Trigger Integration**:
   - Ensure `Hoe` and held tool prefabs have an "Attack" trigger on their `Animator` (or respond to `PlayerHandVisualizer.TriggerAttackAnimation()`), playing a downward slash/swing animation.

## 4. Verification & Testing Checklist
- [ ] Left-clicking with Hoe equipped in `Idle` mode performs an attack swing that damages enemies.
- [ ] Right-clicking switches Hoe to `Farming` mode; left-clicking then tills the soil without triggering an attack.
- [ ] Switching Hoe to `Deleting` mode clears tiles without triggering an attack.
- [ ] Non-attackable items (such as `SeedBag`) do not raise attack events in `Idle` mode.
