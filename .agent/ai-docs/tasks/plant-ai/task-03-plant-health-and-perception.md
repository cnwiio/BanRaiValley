# Task 03: Plant Health & Perception Components

## 1. Task Goal
Implement `PlantHealth` (implementing `IDamageable` with event dispatching on damage/death) and `PlantPerception` (event- and trigger-driven target detection with zero `Update` polling).

---

## 2. Task Information
- **System**: Plant AI System
- **Parent Plan**: [.agent/ai-docs/plan/plant-ai-plan.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/ai-docs/plan/plant-ai-plan.md)
- **Target File(s)**:
  - `Assets/Project/Scripts/AI/PlantAI/PlantHealth.cs`
  - `Assets/Project/Scripts/AI/PlantAI/PlantPerception.cs`
- **Dependencies / Prerequisites**:
  - Task 01 (`IDamageable.cs`, `DamageData.cs`, `EventBus.cs`)
  - Task 02 (`PlantAIConfigSO.cs`)
- **Applicable Rules**:
  - [.agent/rule/architecture-guide.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/architecture-guide.md) (Rule 1: Event-driven, Rule 10: Zero Update polling in AI, Rule 12.1: Event unsubscription in `OnDisable`)
  - [.agent/rule/naming-variable-rule.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/naming-variable-rule.md) (Modal booleans `isAlive`, `isInvulnerable`, `_camelCase` private fields)
  - [.agent/rule/function-rule.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/function-rule.md) (Command vs Query separation)

---

## 3. What To Do (Step-by-Step Instructions)

### Step 1: Implement `PlantHealth.cs`
Create `Assets/Project/Scripts/AI/PlantAI/PlantHealth.cs`:
- Class implements `MonoBehaviour` and `IDamageable`.
- **Public Properties**:
  - `public float CurrentHp => _currentHp;`
  - `public float MaxHp => _maxHp;`
  - `public bool IsAlive => _currentHp > 0f;`
  - `public bool IsInvulnerable => _isInvulnerable;`
- **Events**:
  - `public event Action<DamageData> OnDamaged;`
  - `public event Action OnDied;`
- **Methods**:
  - `public void Initialize(float maxHp)`: Sets `_maxHp = maxHp; _currentHp = maxHp;` and resets invulnerability.
  - `public void SetInvulnerable(bool isInvulnerable)`: Mutator to protect the plant during awakening or stagger.
  - `public void TakeDamage(DamageData damageData)`:
    - Guard against `!IsAlive` or `_isInvulnerable` or `damageData.Amount <= 0`.
    - Subtract `damageData.Amount` from `_currentHp`, clamped to zero.
    - Invoke `OnDamaged?.Invoke(damageData)`.
    - Raise `EventBus<OnPlantDamagedEvent>.Raise(...)`.
    - If `_currentHp <= 0f`, invoke `OnDied?.Invoke()` and raise `EventBus<OnPlantDiedEvent>.Raise(...)`.

### Step 2: Implement `PlantPerception.cs`
Create `Assets/Project/Scripts/AI/PlantAI/PlantPerception.cs`:
- Handles target sensing without polling in `Update`. Uses a dedicated `SphereCollider` set as a trigger zone or discrete interval check.
- **Serialized Fields**:
  - `[SerializeField] private SphereCollider _aggroTrigger;`
  - `[SerializeField] private LayerMask _targetLayer;`
- **Properties**:
  - `public Transform CurrentTarget { get; private set; }`
  - `public bool HasTarget => CurrentTarget != null;`
- **Events**:
  - `public event Action<Transform> OnTargetDetected;`
  - `public event Action OnTargetLost;`
- **Methods & Unity Callbacks**:
  - `public void Initialize(float aggroRadiusM)`: Configures `_aggroTrigger.radius = aggroRadiusM;`
  - `public void SetPerceptionActive(bool isActive)`: Enables/disables `_aggroTrigger`.
  - `private void OnTriggerEnter(Collider other)`:
    - Checks layer mask. If matching target (e.g. Player) and `CurrentTarget == null`:
      - Set `CurrentTarget = other.transform;`
      - Invoke `OnTargetDetected?.Invoke(CurrentTarget);`
  - `private void OnTriggerExit(Collider other)`:
    - If `other.transform == CurrentTarget`:
      - Clear `CurrentTarget = null;`
      - Invoke `OnTargetLost?.Invoke();`
  - `public void ClearTarget()`: Manually resets `CurrentTarget` and fires `OnTargetLost`.

---

## 4. Verification & Testing Checklist
- [ ] No `Update()` method exists in `PlantHealth` or `PlantPerception`.
- [ ] `PlantHealth` properly clamps HP and dispatches both local events and global `EventBus` structs.
- [ ] `PlantPerception` relies strictly on trigger callbacks and explicit activation methods.
- [ ] All public members have XML summary documentation.
