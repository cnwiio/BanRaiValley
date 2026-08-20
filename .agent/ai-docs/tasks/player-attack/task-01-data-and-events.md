# Task 01: Data Models, Item Attack Config & Combat EventBus Signatures

## 1. Task Goal
Define the data structures for attack parameters (`ItemAttackData`), integrate attack configuration into the `Item` ScriptableObject, and add player combat event signatures to the global `EventBus`.

## 2. Task Information
- **System**: Player Attack System
- **Parent Plan**: [.agent/ai-docs/plan/player-attack-plan.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/ai-docs/plan/player-attack-plan.md)
- **Target File(s)**:
  - `Assets/Project/Scripts/Inventory/ItemAttackData.cs` (NEW)
  - `Assets/Project/Scripts/Inventory/Item.cs` (MODIFY)
  - `Assets/Project/Scripts/EventBus.cs` (MODIFY)
- **Dependencies / Prerequisites**: None
- **Applicable Rules**:
  - [.agent/rule/architecture-guide.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/architecture-guide.md)
  - [.agent/rule/naming-variable-rule.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/naming-variable-rule.md)
  - [.agent/rule/function-rule.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/function-rule.md)

## 3. What To Do (Step-by-Step Instructions)

1. **Create `ItemAttackData.cs`**:
   - Location: `Assets/Project/Scripts/Inventory/ItemAttackData.cs`
   - Define a `[System.Serializable] public struct ItemAttackData`:
     - `public bool canAttack;`
     - `public float damageAmount;`
     - `public DamageType damageType;`
     - `public float attackCooldownSec;`
     - `public float staminaCost;`
     - `public float knockbackForce;`
     - `public Vector3 attackBoxSize;`
     - `public Vector3 attackBoxOffset;`
     - Provide a static factory property `public static ItemAttackData DefaultUnarmed => new ItemAttackData { canAttack = true, damageAmount = 2f, damageType = DamageType.Physical, attackCooldownSec = 0.4f, staminaCost = 1f, knockbackForce = 2f, attackBoxSize = new Vector3(1.2f, 1.2f, 2.0f), attackBoxOffset = new Vector3(0f, 0f, 1.2f) };`

2. **Update `Item.cs`**:
   - In `Assets/Project/Scripts/Inventory/Item.cs`:
     - Add `[Header("Combat")]` and `[SerializeField] private ItemAttackData _attackData = ItemAttackData.DefaultUnarmed;`
     - Add a public getter property: `public ItemAttackData AttackData => _attackData;`

3. **Update `EventBus.cs`**:
   - In `Assets/Project/Scripts/EventBus.cs`:
     - Add region `#region Player Combat Events`
     - Define `public struct OnPlayerRequestAttackEvent : IEvent { }`
     - Define `public struct OnPlayerAttackExecutedEvent : IEvent { public Item EquippedItem; public ItemAttackData AttackData; public Vector3 AttackOrigin; }`
     - Define `public struct OnPlayerHitTargetEvent : IEvent { public GameObject TargetInstance; public DamageData DamageData; public bool TargetDied; }`

## 4. Verification & Testing Checklist
- [ ] `ItemAttackData.cs` compiles cleanly.
- [ ] `Item.cs` exposes `AttackData` in inspector with tooltips and sensible defaults.
- [ ] `EventBus.cs` has all new structs registered under `IEvent`.
- [ ] All private fields follow `_camelCase` naming conventions.
