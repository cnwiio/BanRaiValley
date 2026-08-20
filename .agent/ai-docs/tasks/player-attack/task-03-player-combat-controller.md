# Task 03: Player Combat Controller Core & OverlapBox Hit Detection

## 1. Task Goal
Create the `PlayerCombatController` component that manages attack cooldowns, plays attack swing animations via `PlayerHandVisualizer`, performs First-Person `Physics.OverlapBoxNonAlloc` hit detection, and applies `DamageData` to `IDamageable` targets.

## 2. Task Information
- **System**: Player Attack System
- **Parent Plan**: [.agent/ai-docs/plan/player-attack-plan.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/ai-docs/plan/player-attack-plan.md)
- **Target File(s)**:
  - `Assets/Project/Scripts/Combat/PlayerCombatController.cs` (NEW)
- **Dependencies / Prerequisites**:
  - Task 01, Task 02
- **Applicable Rules**:
  - [.agent/rule/architecture-guide.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/architecture-guide.md)
  - [.agent/rule/naming-variable-rule.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/naming-variable-rule.md)
  - [.agent/rule/function-rule.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/function-rule.md)

## 3. What To Do (Step-by-Step Instructions)

1. **Create `PlayerCombatController.cs`**:
   - Location: `Assets/Project/Scripts/Combat/PlayerCombatController.cs`
   - Inherit from `MonoBehaviour`.

2. **Serialized Fields & Configuration**:
   - `[Header("References")]`
     - `[SerializeField] private PlayerHandVisualizer _handVisualizer;`
     - `[SerializeField] private Transform _attackCameraTransform;`
     - `[SerializeField] private LayerMask _hitLayers;`
   - `[Header("Unarmed Fallback")]`
     - `[SerializeField] private ItemAttackData _unarmedAttackData = ItemAttackData.DefaultUnarmed;`
   - `[Header("Hit Detection Settings")]`
     - `[SerializeField] private float _impactDelaySec = 0.1f;` (time before OverlapBox check to match animation swing)
     - `[SerializeField] private int _maxTargetCount = 10;`

3. **Internal State & Caching**:
   - `private float _nextAttackTimeSec;`
   - `private SlotData _currentSlotData;`
   - `private Collider[] _hitColliderBuffer;`
   - `private Coroutine _hitCheckCoroutine;`
   - In `Awake()`:
     - Initialize `_hitColliderBuffer = new Collider[_maxTargetCount];`
     - If `_attackCameraTransform == null && Camera.main != null`, resolve `_attackCameraTransform = Camera.main.transform;`

4. **Event Subscriptions (`OnEnable` / `OnDisable`)**:
   - Subscribe to:
     - `EventBus<OnHotbarChangeEvent>` $\rightarrow$ cache `_currentSlotData = evt.slotData;`
     - `EventBus<OnPlayerRequestAttackEvent>` $\rightarrow$ execute `RequestAttack();`
     - `EventBus<OnPrimaryActionEvent>` $\rightarrow$ if player is unarmed (or holding a pure weapon with no tool override), execute `RequestAttack();`
   - Unsubscribe cleanly in `OnDisable()`.
   - Cancel `_hitCheckCoroutine` in `OnDisable()`.

5. **Attack Execution Logic (`RequestAttack()`)**:
   - Guard condition: Check if `Time.time < _nextAttackTimeSec`. If on cooldown, return early.
   - Resolve current attack data:
     - If `_currentSlotData != null && !_currentSlotData.IsEmpty && _currentSlotData.item != null`:
       - If `!_currentSlotData.item.AttackData.canAttack`, return.
       - Use `_currentSlotData.item.AttackData`.
     - Else:
       - Use `_unarmedAttackData`.
   - Set `_nextAttackTimeSec = Time.time + currentAttackData.attackCooldownSec;`
   - Trigger attack animation on `_handVisualizer` via `_handVisualizer.TriggerAttackAnimation();`
   - Raise `OnPlayerAttackExecutedEvent`.
   - Start delayed coroutine to perform `PerformHitDetection(currentAttackData)`.

6. **OverlapBox Hit Detection (`PerformHitDetection`)**:
   - Calculate box center: `Vector3 boxCenter = _attackCameraTransform.position + _attackCameraTransform.TransformDirection(attackData.attackBoxOffset);`
   - Half extents: `Vector3 halfExtents = attackData.attackBoxSize * 0.5f;`
   - Rotation: `Quaternion orientation = _attackCameraTransform.rotation;`
   - Call `int hitCount = Physics.OverlapBoxNonAlloc(boxCenter, halfExtents, _hitColliderBuffer, orientation, _hitLayers, QueryTriggerInteraction.Ignore);`
   - For each collider found:
     - Check `IDamageable damageable = collider.GetComponentInParent<IDamageable>();`
     - If `damageable != null && damageable.IsAlive`:
       - Construct `DamageData`:
         - `Amount = attackData.damageAmount`
         - `Type = attackData.damageType`
         - `Source = gameObject`
         - `HitPoint = collider.ClosestPoint(boxCenter)`
         - `HitNormal = (_attackCameraTransform.position - collider.transform.position).normalized`
         - `KnockbackForce = attackData.knockbackForce`
       - Call `damageable.TakeDamage(damageData);`
       - Raise `EventBus<OnPlayerHitTargetEvent>.Raise(new OnPlayerHitTargetEvent { TargetInstance = collider.gameObject, DamageData = damageData, TargetDied = !damageable.IsAlive });`

7. **Debug Gizmos**:
   - In `#if UNITY_EDITOR`, implement `OnDrawGizmosSelected()` to draw the attack `OverlapBox` wireframe in scene view for convenient visual tuning.

## 4. Verification & Testing Checklist
- [ ] No allocations in hot paths (`OverlapBoxNonAlloc` with preallocated buffer).
- [ ] Cooldown stops spamming attacks.
- [ ] Hits detect `IDamageable` entities (e.g. `PlantHealth`) correctly and decrease HP.
- [ ] `OnDisable` terminates any pending coroutine and unsubscribes from EventBus.
