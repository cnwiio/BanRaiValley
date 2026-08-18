# Task 05: Plant Brain & State Machine Controller

## 1. Task Goal
Implement the central decision coordinator `PlantBrain` (implementing `IAIAgent`), integrating `PlantHealth`, `PlantPerception`, `PlantMovement`, and `PlantCombat` into an event-driven Finite State Machine (FSM) without polling in `Update`.

---

## 2. Task Information
- **System**: Plant AI System
- **Parent Plan**: [.agent/ai-docs/plan/plant-ai-plan.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/ai-docs/plan/plant-ai-plan.md)
- **Target File(s)**:
  - `Assets/Project/Scripts/AI/PlantAI/PlantBrain.cs`
  - `Assets/Project/Scripts/AI/PlantAI/PlantAnimationController.cs`
- **Dependencies / Prerequisites**:
  - Task 01 (`IAIAgent.cs`, `EventBus.cs`)
  - Task 02 (`PlantAIConfigSO.cs`, `PlantAIState.cs`)
  - Task 03 (`PlantHealth.cs`, `PlantPerception.cs`)
  - Task 04 (`PlantMovement.cs`, `PlantCombat.cs`)
- **Applicable Rules**:
  - [.agent/rule/architecture-guide.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/architecture-guide.md) (Rule 9: State Machines emit events, Rule 10: Zero Update polling in AI, Rule 12.1: Event unsubscription in `OnDisable`)
  - [.agent/rule/function-rule.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/function-rule.md) (Single Responsibility, thin lifecycle methods)

---

## 3. What To Do (Step-by-Step Instructions)

### Step 1: Implement `PlantAnimationController.cs`
Create `Assets/Project/Scripts/AI/PlantAI/PlantAnimationController.cs`:
- Wraps `Animator` parameter setting to eliminate string hashing overhead per frame.
- **Fields**:
  - `[SerializeField] private Animator _animator;`
  - Cached hash IDs (`_speedHash`, `_awakenHash`, `_attackHash`, `_hitHash`, `_dieHash`).
- **Methods**:
  - `public void PlayAwaken()`
  - `public void PlayAttack()`
  - `public void PlayHit()`
  - `public void PlayDie()`
  - `public void SetMoving(bool isMoving)`

### Step 2: Implement `PlantBrain.cs`
Create `Assets/Project/Scripts/AI/PlantAI/PlantBrain.cs`:
- Class implements `MonoBehaviour` and `IAIAgent`.
- **Serialized Components**:
  - `[Header("Configuration")]`
  - `[SerializeField] private PlantAIConfigSO _config;`
  - `[SerializeField] private PlantLootTableSO _lootTable;`
  - `[Header("Internal Subsystems")]`
  - `[SerializeField] private PlantHealth _health;`
  - `[SerializeField] private PlantPerception _perception;`
  - `[SerializeField] private PlantMovement _movement;`
  - `[SerializeField] private PlantCombat _combat;`
  - `[SerializeField] private PlantAnimationController _animationController;`
- **Properties**:
  - `public PlantAIState CurrentState { get; private set; } = PlantAIState.Dormant;`
  - `public Transform TargetTransform => _perception.CurrentTarget;`
- **Lifecycle & Event Subscriptions**:
  - `OnEnable`:
    - Subscribe to `_health.OnDamaged += OnDamaged;`
    - Subscribe to `_health.OnDied += OnDied;`
    - Subscribe to `_perception.OnTargetDetected += OnTargetDetected;`
    - Subscribe to `_perception.OnTargetLost += OnTargetLost;`
    - Subscribe to `_combat.OnAttackCompleted += OnAttackCompleted;`
  - `OnDisable`:
    - Unsubscribe from all above events.
    - Stop all active state coroutines.
- **State Transition Engine**:
  - `public void TransitionTo(PlantAIState nextState)`:
    - If `CurrentState == nextState` return.
    - Exit logic for `CurrentState`.
    - Enter logic for `nextState`.
    - Raise `EventBus<OnPlantStateChangedEvent>.Raise(...)`.
    - Set `CurrentState = nextState;`
- **State Handlers**:
  - `Awaken()`:
    - Transitions to `PlantAIState.Awakening`.
    - Sets health invulnerable.
    - Plays awaken animation.
    - Starts `AwakeningRoutine()` which after `_config.AwakeningDurationSec` removes invulnerability, enables perception, and transitions to `PlantAIState.Idle`.
  - `OnTargetDetected(Transform target)`:
    - If in `Idle` or `Dormant`, transition to `Chase`.
    - Starts `ChaseTargetRoutine(target)`.
  - `ChaseTargetRoutine(Transform target)`:
    - Runs a periodic destination update timer (e.g. every `0.25f` seconds, not per-frame in `Update`).
    - If target within `_config.AttackRangeM` and `_combat.CanAttack`:
      - Stop movement.
      - Transition to `PlantAIState.Attack`.
      - Trigger `_combat.ExecuteMeleeAttack(target)`.
  - `OnAttackCompleted()`:
    - If target still in perception, resume `Chase`. Otherwise return to `Idle`.
  - `OnDamaged(DamageData damageData)`:
    - Play hit animation.
    - If not already attacking, briefly transition to `HitReact` then resume chase.
  - `OnDied()`:
    - Transition to `PlantAIState.Dead`.
    - Stop movement, disable perception and colliders.
    - Play death animation.
    - Drop items via `_lootTable.EvaluateDrops()`.
    - Starts `DespawnRoutine()`.

---

## 4. Verification & Testing Checklist
- [ ] `PlantBrain` does not contain `Update()` polling for AI state decisions.
- [ ] Event subscriptions in `OnEnable` are mirrored by unsubscriptions in `OnDisable`.
- [ ] State transitions emit `OnPlantStateChangedEvent` via `EventBus`.
- [ ] Coroutine references are tracked and stopped on disable.
