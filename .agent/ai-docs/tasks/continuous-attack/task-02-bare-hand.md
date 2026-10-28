# Task 02: Implement Continuous Attack in BareHand

## 1. Task Goal
Update `BareHand.cs` to listen to primary action start/cancel events and forward the hold state to `ComboAttackController`, enabling continuous looping punches while Left Click is held.

## 2. Task Information
- **System**: Combat System / Player Equipment
- **Parent Plan**: [continuous-attack-plan.md](../../plan/continuous-attack-plan.md)
- **Target File(s)**:
  - `Assets/Project/Scripts/Bare Hand.cs`
- **Dependencies / Prerequisites**:
  - Task 01 (`ComboAttackController.SetHoldAttack`)
- **Applicable Rules**:
  - `architecture-guide.md`: Event subscriptions in `OnEnable`/`OnDisable`.
  - `function-rule.md`: Explicit event callbacks.

## 3. What To Do (Step-by-Step Instructions)
1. **Event Subscriptions**:
   - In `OnEnable()`:
     - Subscribe to `OnPrimaryActionStartedEvent` and `OnPrimaryActionCanceledEvent`.
     - Subscribe to `ChangeActionMap`.
   - In `OnDisable()`:
     - Unsubscribe from `OnPrimaryActionStartedEvent` and `OnPrimaryActionCanceledEvent`.
     - Unsubscribe from `ChangeActionMap`.
     - Call `combo.SetHoldAttack(false);`
2. **Event Handlers**:
   - `OnPrimaryActionStarted(OnPrimaryActionStartedEvent evt)`:
     - Call `combo.SetHoldAttack(true);`
   - `OnPrimaryActionCanceled(OnPrimaryActionCanceledEvent evt)`:
     - Call `combo.SetHoldAttack(false);`
   - `OnChangeActionMap(ChangeActionMap evt)`:
     - If `evt.MapType != ActionMapType.Player`, call `combo.SetHoldAttack(false);`
3. **Single Click Handling**:
   - `OnPrimaryAction(OnPrimaryActionEvent evt)`:
     - Call `combo.TryAttack();` (ensures single taps work seamlessly).

## 4. Verification & Testing Checklist
- [ ] Script compiles with zero warnings/errors in Unity 6.3.
- [ ] Single click punches once.
- [ ] Holding LMB punches continuously in a loop (Left $\rightarrow$ Right $\rightarrow$ Left $\rightarrow$ Right).
- [ ] Releasing LMB cleanly returns to Idle.
- [ ] Opening UI (Tab) halts hold attack.
