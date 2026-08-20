# Task 05: Combat Subsystem Documentation & User Manual

## 1. Task Goal
Author a comprehensive `README.md` inside `Assets/Project/Scripts/Combat/` documenting the Player Attack System, architecture overview, data setup, EventBus events, and a complete user manual for setting up weapons, bare hands, and tools in the Unity Inspector.

## 2. Task Information
- **System**: Player Attack System
- **Parent Plan**: [.agent/ai-docs/plan/player-attack-plan.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/ai-docs/plan/player-attack-plan.md)
- **Target File(s)**:
  - `Assets/Project/Scripts/Combat/README.md` (MODIFY)
- **Dependencies / Prerequisites**:
  - Tasks 01–04
- **Applicable Rules**:
  - [.agent/rule/architecture-guide.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/architecture-guide.md) (Rule 16: Readme File requirement)

## 3. What To Do (Step-by-Step Instructions)

1. **Update `Assets/Project/Scripts/Combat/README.md`**:
   - Provide an updated subsystem overview detailing:
     - Player Combat Controller & OverlapBox hit detection.
     - Minecraft-style bare hand fallback visualizer.
     - Integration with `IDamageable` and Plant AI monsters.
     - Context-aware tool attack states (Idle vs Farming).
   - Document all EventBus events:
     - `OnPlayerRequestAttackEvent`
     - `OnPlayerAttackExecutedEvent`
     - `OnPlayerHitTargetEvent`
   - Include a step-by-step User Manual for Unity Designers:
     - How to configure `ItemAttackData` on an `Item` ScriptableObject.
     - How to set up the `_bareHandPrefab` on `PlayerHandVisualizer`.
     - How to adjust the `OverlapBox` size, offset, and layer mask in `PlayerCombatController`.

## 4. Verification & Testing Checklist
- [ ] `README.md` contains both an overview and a comprehensive user manual per rule 16.
- [ ] All code examples, events, and inspector field references match the implemented scripts.
