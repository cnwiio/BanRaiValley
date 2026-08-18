# Task 01: Core Combat Contracts & Event Signatures

## 1. Task Goal
Create foundational, interface-driven combat abstractions (`IDamageable`, `IAIAgent`, `DamageData`, `DamageType`) and register all strongly-typed Plant AI events into `EventBus.cs` to ensure decoupled cross-system communication without direct object dependencies.

---

## 2. Task Information
- **System**: Plant AI System
- **Parent Plan**: [.agent/ai-docs/plan/plant-ai-plan.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/ai-docs/plan/plant-ai-plan.md)
- **Target File(s)**:
  - `Assets/Project/Scripts/Combat/IDamageable.cs`
  - `Assets/Project/Scripts/Combat/IAIAgent.cs`
  - `Assets/Project/Scripts/Combat/DamageData.cs`
  - `Assets/Project/Scripts/Combat/DamageType.cs`
  - `Assets/Project/Scripts/EventBus.cs` (modify to add Plant AI event structs)
- **Dependencies / Prerequisites**:
  - Existing [EventBus.cs](file:///d:/Work/Unity%20Project/BanRaiValley/Assets/Project/Scripts/EventBus.cs)
- **Applicable Rules**:
  - [.agent/rule/architecture-guide.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/architecture-guide.md) (Rule 4: Interface-Driven Architecture, Rule 2: Strong-Typed Events)
  - [.agent/rule/naming-variable-rule.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/naming-variable-rule.md) (Modal booleans, PascalCase interfaces)
  - [.agent/rule/code-style-rule.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/code-style-rule.md) (One class/interface per file, XML docs on public members)

---

## 3. What To Do (Step-by-Step Instructions)

### Step 1: Create `DamageType.cs`
Create `Assets/Project/Scripts/Combat/DamageType.cs`:
- Define `public enum DamageType`:
  - `Physical = 0`
  - `Slashing = 1`
  - `Blunt = 2`
  - `Piercing = 3`
  - `Fire = 4`
  - `Water = 5`

### Step 2: Create `DamageData.cs`
Create `Assets/Project/Scripts/Combat/DamageData.cs`:
- Define `public readonly struct DamageData`:
  - Properties with `{ get; }`:
    - `float Amount`
    - `DamageType Type`
    - `GameObject Source`
    - `Vector3 HitPoint`
    - `Vector3 HitNormal`
    - `float KnockbackForce`
  - Constructor initializing all properties with default `knockbackForce = 0f`.
  - Include XML summary documentation for the struct and its properties.

### Step 3: Create `IDamageable.cs`
Create `Assets/Project/Scripts/Combat/IDamageable.cs`:
- Define `public interface IDamageable`:
  - `float CurrentHp { get; }`
  - `float MaxHp { get; }`
  - `bool IsAlive { get; }`
  - `void TakeDamage(DamageData damageData);`
- Include XML summary for each member.

### Step 4: Create `IAIAgent.cs`
Create `Assets/Project/Scripts/Combat/IAIAgent.cs`:
- Define `public interface IAIAgent`:
  - `Transform TargetTransform { get; }`
  - `PlantAIState CurrentState { get; }`
  - `void SetTarget(Transform target);`
  - `void ClearTarget();`

### Step 5: Update `EventBus.cs` with Plant AI Events
In `Assets/Project/Scripts/EventBus.cs`, append the following event structs conforming to `IEvent`:
- `public struct OnPlantAwakenedEvent : IEvent`
  - `public GameObject PlantInstance;`
  - `public Vector3Int CellPos;`
  - `public Vector3 WorldPosition;`
- `public struct OnPlantStateChangedEvent : IEvent`
  - `public GameObject PlantInstance;`
  - `public PlantAIState PreviousState;`
  - `public PlantAIState NewState;`
- `public struct OnPlantDamagedEvent : IEvent`
  - `public GameObject PlantInstance;`
  - `public DamageData DamageData;`
  - `public float CurrentHp;`
  - `public float MaxHp;`
- `public struct OnPlantDiedEvent : IEvent`
  - `public GameObject PlantInstance;`
  - `public Vector3 Position;`
  - `public Vector3Int CellPos;`
- `public struct OnPlantAttackExecutedEvent : IEvent`
  - `public GameObject PlantInstance;`
  - `public Transform TargetTransform;`
  - `public float DamageAmount;`

---

## 4. Verification & Testing Checklist
- [ ] All new interfaces and structs are in separate files in `Assets/Project/Scripts/Combat/`.
- [ ] `EventBus.cs` compiles cleanly with the new `IEvent` structs.
- [ ] All public members have standard XML `/// <summary>` documentation.
- [ ] All private/internal fields adhere to `_camelCase` and properties to `PascalCase`.
