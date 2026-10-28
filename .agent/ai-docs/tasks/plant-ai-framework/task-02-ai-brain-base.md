# Task 02: Create `AIBrainBase.cs` Coordinator

## 1. Task Goal
Implement `AIBrainBase.cs`, the abstract coordinator class for all plant monster AI. It manages the plant state machine, orchestrates the modular base components, handles event subscription/unsubscription without leaks, provides virtual hooks for derived brains, and interfaces cleanly with `LeanPool` (`IPoolable`) and `EventBus`.

## 2. Task Information
- **System**: Plant AI System
- **Parent Plan**: [plant-ai-framework-plan.md](../../plan/plant-ai-framework-plan.md)
- **Target File(s)**:
  - `Assets/Project/Scripts/AI/AIBrainBase.cs`
- **Dependencies / Prerequisites**:
  - Task 01: Base component interfaces and abstract classes.
- **Applicable Rules**:
  - `architecture-guide.md`: Section 3 (Max inheritance depth 2), Section 8/13 (Pool lifecycle), Section 9 (State transitions emit events), Section 12 (Zero leaks in OnEnable/OnDisable).
  - `code-style-rule.md`: `#region` blocks, XML documentation, `[Header]` and `[Tooltip]` on serialized fields.
  - `function-rule.md`: Lifecycle methods as coordinators only, thin methods.

## 3. What To Do (Step-by-Step Instructions)

Create `Assets/Project/Scripts/AI/AIBrainBase.cs`:

1. **State Enums**:
   - `PlantAIState` (`Awake`, `Idle`, `Chase`, `Attack`, `Stun`, `Die`)
   - `PlantChaseState` (`ChasePlayer`, `ChaseHome`)

2. **Component References**:
   - Protected serialized fields with `[Header]` and `[Tooltip]`:
     ```csharp
     [Header("Data")]
     [Tooltip("Configuration data asset specifying stats, drops, and intervals.")]
     [SerializeField] protected AIData data;

     [Header("Modular Components")]
     [Tooltip("Locomotion and navigation component.")]
     [SerializeField] protected AIMovementBase movement;

     [Tooltip("Perception and player detection component.")]
     [SerializeField] protected AIDetectionBase detection;

     [Tooltip("Combat and damage delivery component.")]
     [SerializeField] protected AIAttackBase attack;

     [Tooltip("Skeletal animation coordinator component.")]
     [SerializeField] protected AIAnimationControllerBase animator;

     [Tooltip("Health, damage reception, and death handler.")]
     [SerializeField] protected AIHealthBase health;
     ```

3. **Auto-Resolution in `Awake()`**:
   - If any component reference is null, attempt `GetComponent<T>()` so prefabs auto-wire cleanly.

4. **State Machine Properties & Encapsulation**:
   ```csharp
   public PlantAIState CurrentState { get; protected set; } = PlantAIState.Idle;
   public PlantChaseState CurrentChaseState { get; protected set; } = PlantChaseState.ChasePlayer;

   public virtual void ChangeState(PlantAIState nextState)
   {
       if (CurrentState == nextState) return;
       OnExitState(CurrentState);
       CurrentState = nextState;
       OnEnterState(nextState);
   }

   public virtual void ChangeChaseState(PlantChaseState nextChaseState)
   {
       ExitChaseState(CurrentChaseState);
       CurrentChaseState = nextChaseState;
       EnterChaseState(nextChaseState);
   }
   ```

5. **Lifecycle & Event Subscriptions**:
   - In `OnEnable()`:
     - `detection.OnTargetDetectedEvent += OnTargetDetected;`
     - `detection.OnTargetLostEvent += OnTargetLost;`
     - `attack.OnAttackEndEvent += OnAttackEnd;`
     - `health.OnTakeDamageEvent += OnTakeDamage;`
     - `health.OnDieEvent += OnDie;`
   - In `OnDisable()`:
     - Strict unsubscription of all 5 events to guarantee zero event leaks.

6. **Virtual State Hooks**:
   - `protected virtual void OnEnterState(PlantAIState state)`
   - `protected virtual void OnExitState(PlantAIState state)`
   - `protected virtual void EnterChaseState(PlantChaseState state)`
   - `protected virtual void ExitChaseState(PlantChaseState state)`

7. **Virtual Event Handlers**:
   - `protected virtual void OnTargetDetected(Transform target)`
   - `protected virtual void OnTargetLost()`
   - `protected virtual void OnAttackEnd()`
   - `public virtual void OnAttackAnimationHit()`
   - `public virtual void OnAwakeAnimationEnd()`
   - `protected virtual void OnTakeDamage()`
   - `protected virtual void OnDie()`

8. **Drop Handling & Pool Hooks**:
   - `protected virtual void RollDrop()`: Evaluates `data.dropChance` and raises `EventBus<OnItemPickupEvent>`.
   - `public virtual void OnSpawn()`: Calls `Initialize()`, sets state to `Awake`.
   - `public virtual void OnDespawn()`: Resets animator and clears state.

## 4. Verification & Testing Checklist
- [ ] `AIBrainBase.cs` compiles cleanly without errors.
- [ ] All serialized fields have `[Header]` and `[Tooltip]`.
- [ ] In `OnDisable()`, every event registered in `OnEnable()` is cleanly unsubscribed.
- [ ] Guard clauses exist on all state changes and target references.
