# Task 04: Refactor `AIBrain.cs` to Inherit from `AIBrainBase`

## 1. Task Goal
Refactor `AIBrain.cs` to inherit from `AIBrainBase`, delegating lifecycle and event coordination to the base class while focusing strictly on the standard plant monster coroutines (`ChaseTargetCoroutine`, `AnimationSpeedCoroutine`, `StunCoroutine`, `DieCoroutine`). Maintain the existing GUID (`129c327fbf3a1984bbbf882c949282f4`) so existing Cabbage Monster prefabs function immediately without re-assignment.

## 2. Task Information
- **System**: Plant AI System
- **Parent Plan**: [plant-ai-framework-plan.md](../../plan/plant-ai-framework-plan.md)
- **Target File(s)**:
  - `Assets/Project/Scripts/AI/AI Brain.cs`
- **Dependencies / Prerequisites**:
  - Task 02: `AIBrainBase.cs`
  - Task 03: Concrete AI components refactored.
- **Applicable Rules**:
  - `architecture-guide.md`: Section 3 (Max inheritance depth 2: MonoBehaviour -> AIBrainBase -> AIBrain), Section 12.2 (Coroutine cleanup).
  - `code-style-rule.md`: Standard `#region` order, XML documentation, `[Header]` and `[Tooltip]`.
  - `function-rule.md`: Thin lifecycle methods, no logic in Update.

## 3. What To Do (Step-by-Step Instructions)

1. **Inheritance & Class Definition**:
   ```csharp
   public class AIBrain : AIBrainBase
   ```
   (Inherits `IPoolable` and base serialized fields `movement`, `detection`, `attack`, `animator`, `health`, `data` from `AIBrainBase`).

2. **Initialization**:
   - Implement `protected override void Initialize()`:
     ```csharp
     protected override void Initialize()
     {
         if (data == null)
         {
             Debug.LogError($"[AIBrain] Missing AIData on {gameObject.name}");
             return;
         }

         detection.Initialize(data.DetectionRange);
         movement.Initialize(transform.position, data);
         attack.Initialize(data);
         health.Initialize(data.HP);
         _lastKnownTargetPos = Vector3.zero;
     }
     ```

3. **State Hook Overrides**:
   - Override `OnEnterState(PlantAIState state)`:
     - `Awake`: Set `health.IsInvincible = true;`, `detection.EnableDetect(false);`, `animator.PlayAwaken();`.
     - `Idle`: Idle animation/standby.
     - `Chase`: Start `movementCoroutine = StartCoroutine(AnimationSpeedCoroutine());`.
     - `Attack`: Call `movement.RotateToTarget();`, `attack.StartAttack();`, `animator.PlayAttack();`.
     - `Stun`: Stop movement, call `attack.StopAttack();`, `animator.PlayHit();`, start `stunCoroutine = StartCoroutine(StunCoroutine());`.
     - `Die`: Stop movement, call `attack.StopAttack();`, `animator.PlayDie();`, start `dieCoroutine = StartCoroutine(DieCoroutine());`.
   - Override `OnExitState(PlantAIState state)`:
     - Cleanly stop and nullify any active coroutine (`chaseCoroutine`, `movementCoroutine`, `stunCoroutine`, `dieCoroutine`).

4. **Chase Sub-State Overrides**:
   - `EnterChaseState(PlantChaseState state)`:
     - `ChasePlayer`: Start `chaseCoroutine = StartCoroutine(ChaseTargetCoroutine());`.
     - `ChaseHome`: Call `movement.ReturnToStart();`.
   - `ExitChaseState(PlantChaseState state)`:
     - Stop and nullify `chaseCoroutine`.

5. **Coroutine Hardening**:
   - `ChaseTargetCoroutine()`:
     - Guard against null `movement.TargetTransform`.
     - Evaluate distance to target:
       - If within `data.AttackRange`, transition to `PlantAIState.Attack`.
       - Else if target moved beyond `MOVE_THRESHOLD` (1.5f), call `movement.MoveTo(target.position)`.
   - `AnimationSpeedCoroutine()`:
     - Feed `movement.Speed` into `animator.SetMoving()`.
     - If in `ChaseHome` and `movement.IsReachDestination`, transition to `PlantAIState.Idle`.
   - `StunCoroutine()`:
     - Yield `WaitForSeconds(data.StunTime)`.
     - Transition back to `Chase` (with `ChasePlayer` if player in sight, else `ChaseHome`).
   - `DieCoroutine()`:
     - Yield `WaitForSeconds(DE_SPAWN_DELAY_SEC)` (3s).
     - Call `RollDrop()`.
     - Call `LeanPool.Despawn(this)`.

6. **Cleanup on Disable**:
   - In `OnDisable()`, guarantee all 4 coroutines are stopped if active.

## 4. Verification & Testing Checklist
- [ ] `AIBrain.cs` compiles cleanly.
- [ ] Inherits from `AIBrainBase` with zero compile warnings.
- [ ] Coroutines are properly stopped upon state exits and GameObject disabling.
- [ ] Prefab GUID for `AI Brain.cs` remains unchanged.
