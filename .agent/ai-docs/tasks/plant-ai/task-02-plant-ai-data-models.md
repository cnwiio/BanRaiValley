# Task 02: Plant AI ScriptableObject Configurations & Loot Models

## 1. Task Goal
Implement pure data container ScriptableObjects (`PlantAIConfigSO`, `PlantLootTableSO`) and state definitions (`PlantAIState`) to store monster balance metrics and loot drop specifications without runtime state pollution.

---

## 2. Task Information
- **System**: Plant AI System
- **Parent Plan**: [.agent/ai-docs/plan/plant-ai-plan.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/ai-docs/plan/plant-ai-plan.md)
- **Target File(s)**:
  - `Assets/Project/Scripts/AI/PlantAI/PlantAIState.cs`
  - `Assets/Project/Scripts/AI/PlantAI/PlantAIConfigSO.cs`
  - `Assets/Project/Scripts/AI/PlantAI/PlantLootTableSO.cs`
- **Dependencies / Prerequisites**:
  - Task 01 must be completed (or `DamageType.cs` must exist).
  - Existing [Item.cs](file:///d:/Work/Unity%20Project/BanRaiValley/Assets/Project/Scripts/Inventory/Item.cs) for loot entries.
- **Applicable Rules**:
  - [.agent/rule/architecture-guide.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/architecture-guide.md) (Rule 7: ScriptableObject = Data Only, no runtime state/scene references)
  - [.agent/rule/naming-variable-rule.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/naming-variable-rule.md) (Physical units in variable names: `_aggroRadiusM`, `_moveSpeedUps`, `_attackCooldownSec`)
  - [.agent/rule/code-style-rule.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/code-style-rule.md) (`[Header]`, `[Tooltip]`, `[field: SerializeField]`)

---

## 3. What To Do (Step-by-Step Instructions)

### Step 1: Create `PlantAIState.cs`
Create `Assets/Project/Scripts/AI/PlantAI/PlantAIState.cs`:
- Define `public enum PlantAIState`:
  - `Dormant = 0` (Buried / static crop)
  - `Awakening = 1` (Uprooting animation)
  - `Idle = 2` (Wandering / waiting)
  - `Chase = 3` (Pursuing target)
  - `Attack = 4` (Executing combat attack)
  - `HitReact = 5` (Damage stagger / flinch)
  - `Dead = 6` (Defeated, awaiting despawn)

### Step 2: Create `PlantAIConfigSO.cs`
Create `Assets/Project/Scripts/AI/PlantAI/PlantAIConfigSO.cs`:
- Class derives from `ScriptableObject`.
- Add attribute `[CreateAssetMenu(fileName = "PlantAIConfig", menuName = "BanRaiValley/AI/Plant AI Config")]`.
- Fields with `[field: SerializeField]`, `[Header]`, and `[Tooltip]`:
  - `[Header("Health & Defense")]`
    - `public float MaxHp { get; private set; } = 50f;`
    - `public float HitStaggerDurationSec { get; private set; } = 0.3f;`
  - `[Header("Locomotion & Range")]`
    - `public float MoveSpeedUps { get; private set; } = 3.5f;`
    - `public float RotationSpeedDeg { get; private set; } = 360f;`
    - `public float AggroRadiusM { get; private set; } = 8f;`
    - `public float AttackRangeM { get; private set; } = 1.8f;`
    - `public float StoppingDistanceM { get; private set; } = 1.2f;`
  - `[Header("Combat & Timing")]`
    - `public float BaseAttackDamage { get; private set; } = 10f;`
    - `public float AttackCooldownSec { get; private set; } = 1.5f;`
    - `public float AttackWindupSec { get; private set; } = 0.4f;`
    - `public float AwakeningDurationSec { get; private set; } = 1.2f;`
  - `[Header("Animation & Feedback")]`
    - `public string AwakeningTriggerName { get; private set; } = "Awaken";`
    - `public string AttackTriggerName { get; private set; } = "Attack";`
    - `public string HitTriggerName { get; private set; } = "Hit";`
    - `public string DieTriggerName { get; private set; } = "Die";`

### Step 3: Create `PlantLootTableSO.cs`
Create `Assets/Project/Scripts/AI/PlantAI/PlantLootTableSO.cs`:
- Class derives from `ScriptableObject`.
- Add attribute `[CreateAssetMenu(fileName = "PlantLootTable", menuName = "BanRaiValley/AI/Plant Loot Table")]`.
- Define `[System.Serializable] public struct LootDropEntry`:
  - `[Tooltip("Reference to the dropped item data.")] public Item Item;`
  - `[Tooltip("Minimum drop quantity.")] public int MinQuantity;`
  - `[Tooltip("Maximum drop quantity.")] public int MaxQuantity;`
  - `[Range(0f, 100f), Tooltip("Drop chance percentage (0-100%).")] public float DropChancePercent;`
- Expose serialized `[SerializeField] private List<LootDropEntry> _dropEntries = new List<LootDropEntry>();`
- Public read-only property `public IReadOnlyList<LootDropEntry> DropEntries => _dropEntries;`
- Add method `public List<Item> EvaluateDrops()` that evaluates drop chances using `UnityEngine.Random.Range(0f, 100f)` and returns rolled item rewards (pure query, no side effects).

---

## 4. Verification & Testing Checklist
- [ ] ScriptableObjects contain no scene references, event subscriptions, or runtime mutation variables.
- [ ] All serialized fields have clear headers and tooltips with explicit units (`Ups`, `Sec`, `M`, `Deg`, `Percent`).
- [ ] Code follows standard Allman style and XML summary guidelines.
