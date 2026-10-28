# Task 01: Add Hold-Attack Support to ComboAttackController

## 1. Task Goal
Extend `ComboAttackController` with hold-to-attack logic so that when `SetHoldAttack(true)` is set, attacks loop continuously through the combo chain without stopping until `SetHoldAttack(false)` is called.

## 2. Task Information
- **System**: Combat System
- **Parent Plan**: [continuous-attack-plan.md](../../plan/continuous-attack-plan.md)
- **Target File(s)**:
  - `Assets/Project/Scripts/Combat/ComboAttackController.cs`
- **Dependencies / Prerequisites**:
  - None
- **Applicable Rules**:
  - `architecture-guide.md`: Single responsibility, composition over inheritance.
  - `naming-variable-rule.md`: Booleans with modal verbs (`isHoldingAttack`).
  - `function-rule.md`: Command vs Query separation.

## 3. What To Do (Step-by-Step Instructions)
1. **Field Definitions**:
   - Add private field: `private bool isHoldingAttack;`
2. **Hold Method**:
   - Add public method:
     ```csharp
     public void SetHoldAttack(bool isHolding)
     {
         isHoldingAttack = isHolding;
         if (isHolding && !isAttacking)
         {
             TryAttack();
         }
     }
     ```
3. **Looping in `OnAnimationHit()`**:
   - When `OnAnimationHit()` is called:
     - Advance index: `currentIndex = (currentIndex + 1) % attackHashes.Length;`
     - Reset previous trigger.
     - Set `isAttacking = false;`
     - If `isHoldingAttack`:
       - Clear `pendingAttack = false;`
       - Immediately invoke `Attack();` to trigger the next strike in the combo.
     - Else if `pendingAttack`:
       - Clear `pendingAttack = false;`
       - Invoke `Attack();`
4. **Reset Method**:
   - In `Reset()`:
     - Set `isHoldingAttack = false;`

## 4. Verification & Testing Checklist
- [ ] Script compiles with zero warnings/errors in Unity 6.3.
- [ ] Single click still triggers normal single attack / buffered 2-hit combo.
- [ ] Setting hold attack to true continuously cycles combo hits.
- [ ] Setting hold attack to false stops attack looping after current animation completes.
