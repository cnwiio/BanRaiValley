# Task 04: Plant Action Components — Movement & Combat

## 1. Task Goal
Implement the Action layer components: `PlantMovement` (encapsulating `UnityEngine.AI.NavMeshAgent` and rotation logic) and `PlantCombat` (managing attack sequences, cooldown timers, melee hitbox queries, and projectile launching).

---

## 2. Task Information
- **System**: Plant AI System
- **Parent Plan**: [.agent/ai-docs/plan/plant-ai-plan.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/ai-docs/plan/plant-ai-plan.md)
- **Target File(s)**:
  - `Assets/Project/Scripts/AI/PlantAI/PlantMovement.cs`
  - `Assets/Project/Scripts/AI/PlantAI/PlantCombat.cs`
  - `Assets/Project/Scripts/AI/PlantAI/PlantMeleeHitbox.cs`
- **Dependencies / Prerequisites**:
  - Task 01 (`IDamageable.cs`, `DamageData.cs`, `EventBus.cs`)
  - Task 02 (`PlantAIConfigSO.cs`)
- **Applicable Rules**:
  - [.agent/rule/architecture-guide.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/architecture-guide.md) (Rule 3: Composition over Inheritance, Rule 10: Zero Update polling in AI, Rule 12.2: Coroutine cleanup)
  - [.agent/rule/function-rule.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/function-rule.md) (Rule 9: Coroutine naming `...Routine`)
  - [.agent/rule/code-style-rule.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/code-style-rule.md) (`[SerializeField] private`, standard member order)

---

## 3. What To Do (Step-by-Step Instructions)

### Step 1: Implement `PlantMovement.cs`
Create `Assets/Project/Scripts/AI/PlantAI/PlantMovement.cs`:
- Class requires `[RequireComponent(typeof(UnityEngine.AI.NavMeshAgent))]`.
- **Fields**:
  - `[SerializeField] private UnityEngine.AI.NavMeshAgent _navMeshAgent;`
  - `private float _rotationSpeedDeg;`
- **Methods**:
  - `public void Initialize(float speedUps, float stoppingDistM, float rotationSpeedDeg)`:
    - Sets `_navMeshAgent.speed = speedUps;`
    - Sets `_navMeshAgent.stoppingDistance = stoppingDistM;`
    - Sets `_rotationSpeedDeg = rotationSpeedDeg;`
  - `public void SetDestination(Vector3 targetPosition)`:
    - Checks if agent is active and on NavMesh, sets destination.
  - `public void StopMovement()`:
    - Resets path and sets `_navMeshAgent.isStopped = true;`
  - `public void ResumeMovement()`:
    - Sets `_navMeshAgent.isStopped = false;`
  - `public void RotateTowards(Vector3 targetWorldPos)`:
    - Rotates character smoothly toward target without jitter.
  - `public bool IsAtDestination()`:
    - Returns `!_navMeshAgent.pathPending && _navMeshAgent.remainingDistance <= _navMeshAgent.stoppingDistance;`

### Step 2: Implement `PlantMeleeHitbox.cs`
Create `Assets/Project/Scripts/AI/PlantAI/PlantMeleeHitbox.cs`:
- Component attached to attack hitbox collider.
- **Fields**:
  - `[SerializeField] private Collider _hitboxCollider;`
  - `private float _damageAmount;`
  - `private GameObject _owner;`
- **Methods**:
  - `public void Initialize(GameObject owner, float damage)`: Assigns owner and damage.
  - `public void EnableHitbox(bool isEnabled)`: Enables or disables `_hitboxCollider`.
  - `private void OnTriggerEnter(Collider other)`:
    - Guards against self-damage (`other.gameObject == _owner`).
    - Queries `other.GetComponent<IDamageable>()`.
    - If valid, constructs `DamageData` and calls `damageable.TakeDamage(damageData)`.

### Step 3: Implement `PlantCombat.cs`
Create `Assets/Project/Scripts/AI/PlantAI/PlantCombat.cs`:
- Manages attack orchestration and cooldown timer.
- **Serialized Fields**:
  - `[SerializeField] private PlantMeleeHitbox _meleeHitbox;`
  - `[SerializeField] private Transform _projectileSpawnPoint;`
- **Properties**:
  - `public bool CanAttack => _canAttack;`
  - `public bool IsAttacking => _isAttacking;`
- **Events**:
  - `public event Action OnAttackStarted;`
  - `public event Action OnAttackCompleted;`
- **Methods**:
  - `public void Initialize(PlantAIConfigSO config, GameObject owner)`: Initializes hitbox and attack stats.
  - `public void ExecuteMeleeAttack(Transform target)`:
    - Guard against `!_canAttack || _isAttacking`.
    - Starts `MeleeAttackRoutine(target)`.
  - `private IEnumerator MeleeAttackRoutine(Transform target)`:
    - Sets `_isAttacking = true; _canAttack = false;`
    - Fires `OnAttackStarted?.Invoke();`
    - Yields `WaitForSeconds(_config.AttackWindupSec)`.
    - Enables `_meleeHitbox.EnableHitbox(true)`.
    - Yields `WaitForSeconds(0.2f)`.
    - Disables `_meleeHitbox.EnableHitbox(false)`.
    - Sets `_isAttacking = false;`
    - Fires `OnAttackCompleted?.Invoke();`
    - Starts cooldown timer `AttackCooldownRoutine()`.
  - `private IEnumerator AttackCooldownRoutine()`:
    - Yields `WaitForSeconds(_config.AttackCooldownSec)`.
    - Sets `_canAttack = true;`
  - `public void StopAllCombatRoutines()`:
    - Stops active attack/cooldown coroutines safely and disables hitboxes.

---

## 4. Verification & Testing Checklist
- [ ] Coroutines are properly tracked in private fields and stopped in `OnDisable` to prevent memory leaks.
- [ ] No polling inside `Update`.
- [ ] Hitbox checks against `IDamageable` interface without hardcoded player component types.
- [ ] All public methods and properties are documented with XML summaries.
