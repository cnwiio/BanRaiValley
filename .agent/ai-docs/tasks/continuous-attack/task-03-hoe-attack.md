# Task 03: Implement Continuous Attack in Hoe Combat Mode

## 1. Task Goal
Update `Hoe.cs` so that when in Combat/Idle mode (`HoeState.Idle` or `HoeState.Attacking`), holding Left Mouse Button continuously cycles through the 3-hit hoe combo attack while keeping Farming mode (`HoeState.Farming`) completely intact.

## 2. Task Information
- **System**: Farming Tool & Combat
- **Parent Plan**: [continuous-attack-plan.md](../../plan/continuous-attack-plan.md)
- **Target File(s)**:
  - `Assets/Project/Scripts/Farming/Tool/Hoe.cs`
- **Dependencies / Prerequisites**:
  - Task 01 (`ComboAttackController.SetHoldAttack`)
- **Applicable Rules**:
  - `architecture-guide.md`: Event subscriptions in `OnEnable`/`OnDisable`, state machine encapsulation.
  - `naming-variable-rule.md`: Modal prefixes for booleans.

## 3. What To Do (Step-by-Step Instructions)
1. **Event Subscriptions**:
   - In `OnEnable()`:
     - Subscribe to `OnPrimaryActionStartedEvent` and `OnPrimaryActionCanceledEvent`.
   - In `OnDisable()`:
     - Unsubscribe from `OnPrimaryActionStartedEvent` and `OnPrimaryActionCanceledEvent`.
     - Call `combo.SetHoldAttack(false);`
2. **Event Handlers**:
   - `OnPrimaryActionStarted(OnPrimaryActionStartedEvent evt)`:
     - If `CurrentState == HoeState.Idle || CurrentState == HoeState.Attacking`:
       - `CurrentState = HoeState.Attacking;`
       - `combo.SetHoldAttack(true);`
   - `OnPrimaryActionCanceled(OnPrimaryActionCanceledEvent evt)`:
     - `combo.SetHoldAttack(false);`
3. **State Transition on Hit**:
   - In `OnAttackAnimationHit()`:
     - `combo.OnAnimationHit();`
     - `EventBus<OnPlayerRequestAttackEvent>.Raise(new OnPlayerRequestAttackEvent());`
     - If `!combo.IsAttacking()`: `CurrentState = HoeState.Idle;`

## 4. Verification & Testing Checklist
- [ ] Script compiles with zero warnings/errors in Unity 6.3.
- [ ] In Combat/Idle mode (Right Click toggle), holding LMB cycles through the 3-hit hoe combo continuously.
- [ ] Releasing LMB stops attack loop and returns to Idle.
- [ ] In Farming mode (Right Click toggle), holding or clicking LMB tills soil normally without triggering attacks.
