# Task 03: Refactor Concrete AI Components to Inherit Base Classes

## 1. Task Goal
Refactor the existing Cabbage AI components (`AIMovement.cs`, `AIAttack.cs`, `AIDetection.cs`, `AIAnimationController.cs`, `AIHealth.cs`) to inherit from their respective abstract base classes (`AIMovementBase`, `AIAttackBase`, `AIDetectionBase`, `AIAnimationControllerBase`, `AIHealthBase`). Preserve all existing GUIDs and serialized fields so that existing prefabs continue to work seamlessly without losing scene/prefab inspector bindings.

## 2. Task Information
- **System**: Plant AI System
- **Parent Plan**: [plant-ai-framework-plan.md](../../plan/plant-ai-framework-plan.md)
- **Target File(s)**:
  - `Assets/Project/Scripts/AI/AIMovement.cs`
  - `Assets/Project/Scripts/AI/AIAttack.cs`
  - `Assets/Project/Scripts/AI/AIDetection.cs`
  - `Assets/Project/Scripts/AI/AIAnimationController.cs`
  - `Assets/Project/Scripts/AI/AIHealth.cs`
- **Dependencies / Prerequisites**:
  - Task 01: Abstract base classes created.
- **Applicable Rules**:
  - `architecture-guide.md`: Section 3 (Composition over inheritance), Section 12 (Zero leak tolerance for coroutines and events).
  - `code-style-rule.md`: Standard `#region` order, XML documentation, `[Header]` and `[Tooltip]`.
  - `function-rule.md`: Strict Command vs Query separation.

## 3. What To Do (Step-by-Step Instructions)

### 3.1. `AIMovement.cs`
- Update declaration to:
  ```csharp
  [RequireComponent(typeof(NavMeshAgent))]
  public class AIMovement : AIMovementBase
  ```
- Override abstract properties:
  - `public override float Speed => agent != null ? agent.velocity.magnitude : 0f;`
  - `public override bool IsReachDestination => agent != null && !agent.hasPath;`
  - `public override Transform TargetTransform => targetTransform;`
- Override abstract methods:
  - `public override void Initialize(Vector3 startPosition, AIData data)`
  - `public override void BindTargetTransform(Transform target)`
  - `public override void UnBindTargetTransform()`
  - `public override void MoveTo(Vector3 targetPos)`
  - `public override void ReturnToStart()`
  - `public override void StopMoving()`
  - `public override void RotateToTarget()`
- Add null-checks for `agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh`.

### 3.2. `AIAttack.cs`
- Update declaration to:
  ```csharp
  public class AIAttack : AIAttackBase
  ```
- Override properties:
  - `public override bool CanAttack => _canAttack;`
  - `public override bool IsAttacking => _isAttacking;`
- Override event:
  - `public override event Action OnAttackEndEvent;`
- Override methods:
  - `public override void Initialize(AIData data)`: Configures cooldown, hitbox lifespan, and initializes `hitBox.Initialize(data.Damage)`.
  - `public override void StartAttack()`
  - `public override void ExecuteAttack()`
  - `public override void StopAttack()`: Ensures coroutines (`_attackCoroutine`, `_cooldownCoroutine`) are stopped and hitbox is disabled.
- Add `OnDisable()`: Stop all running coroutines to prevent coroutine leaks when despawned.

### 3.3. `AIDetection.cs`
- Update declaration to:
  ```csharp
  public class AIDetection : AIDetectionBase
  ```
- Override properties:
  - `public override bool IsPlayerInSight => _playerTransform != null;`
  - `public override Transform TargetTransform => _playerTransform;`
- Override events:
  - `public override event Action<Transform> OnTargetDetectedEvent;`
  - `public override event Action OnTargetLostEvent;`
- Override methods:
  - `public override void Initialize(float detectionRange)`: Sets `_collider.radius = detectionRange;`
  - `public override void EnableDetect(bool isEnabled)`: Enables/disables trigger collider and clears target if false.

### 3.4. `AIAnimationController.cs`
- Update declaration to:
  ```csharp
  public class AIAnimationController : AIAnimationControllerBase
  ```
- Override methods:
  - `public override void PlayAwaken()`
  - `public override void PlayAttack()`
  - `public override void PlayHit()`
  - `public override void PlayDie()`
  - `public override void SetMoving(float speed)`
  - `public override void Reset()`

### 3.5. `AIHealth.cs`
- Update declaration to:
  ```csharp
  public class AIHealth : AIHealthBase
  ```
- Override properties:
  - `public override bool IsInvincible { get; set; }`
  - `public override int CurrentHp => _hp;`
  - `public override int MaxHp => _maxHp;`
- Override events:
  - `public override event Action OnDieEvent;`
  - `public override event Action OnTakeDamageEvent;`
- Override methods:
  - `public override void Initialize(int maxHp)`
  - `public override void TakeDamage(int amount)`

## 4. Verification & Testing Checklist
- [ ] All 5 concrete scripts compile with 0 errors.
- [ ] Existing `.meta` files are preserved untouched so GUIDs remain identical.
- [ ] Coroutines stop cleanly in `OnDisable()`.
- [ ] Code formatting complies with `#region`, XML docs, and Allman braces.
