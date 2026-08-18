
## C# Coding Style Rules
### For Coder Agent — OOP / Unity 6.3 / Clean & Optimized

### Core Philosophy
Code is written once but read hundreds of times. Style rules exist to make the **reader's** job effortless. Consistency, clarity, and intent must be visible without running the code.

---

### 1. Casing & Naming Conventions

These complement the Variable and Function rules. Casing is the first signal of a symbol's role.

| Symbol | Convention | Example |
|---|---|---|
| Class / Struct / Enum | `PascalCase` | `PlayerController`, `DamageData` |
| Interface | `I` + `PascalCase` | `IDamageable`, `IPoolable` |
| Public method | `PascalCase` | `ApplyDamage()` |
| Private method | `PascalCase` | `RecalculateStats()` |
| Public property | `PascalCase` | `CurrentHp` |
| Local variable | `camelCase` | `finalDamage` |
| Private / cached field | `_camelCase` | `_rigidbody`, `_cachedTransform` |
| Constant | `UPPER_SNAKE_CASE` | `MAX_ENEMY_COUNT` |
| Enum value | `PascalCase` | `GameState.Playing` |

---

### 2. Class Member Order

Every class must follow this exact top-to-bottom order. Use `#region` blocks to enforce it visually (see Rule 8).

```
1. Constants & static fields
2. [Header] + [SerializeField] private fields
3. Public auto-properties
4. Private fields (runtime state, cached refs)
5. Unity Messages (Awake, OnEnable, Start, Update…)
6. Public methods
7. Private methods
8. Coroutines
9. Event handlers (On… methods)
```

One blank line between each section. Two blank lines before a new `#region`.

---

### 3. Indentation & Formatting

- **4 spaces** per indent level — no tabs, ever.
- **Allman brace style** for Unity C#: opening brace on its **own line**. This is the Microsoft C# standard and Unity's own convention.

```csharp
// ✅ Allman — correct for Unity C#
public void ApplyDamage(float amount)
{
    if (amount <= 0)
    {
        return;
    }
    _currentHp -= amount;
}

// ❌ K&R — incorrect for this standard
public void ApplyDamage(float amount) {
    ...
}
```

- Maximum **one statement per line**.
- Maximum **120 characters per line**. Wrap with intent, not arbitrarily.
- No trailing whitespace.

---

### 4. SerializeField — Always Use `[Header]` and `[Tooltip]`

`public` fields are forbidden for Unity-inspector-exposed values. Use `[SerializeField] private` exclusively. Every serialized block must have a `[Header]` and every field must have a `[Tooltip]`.

```csharp
// ❌ Forbidden
public float speed;
public GameObject bulletPrefab;

// ✅ Correct
[Header("Movement")]
[Tooltip("Movement speed in units per second.")]
[SerializeField] private float _moveSpeedUps = 5f;

[Header("Combat")]
[Tooltip("Prefab spawned when the player fires. Must have a Projectile component.")]
[SerializeField] private GameObject _bulletPrefab;

[Tooltip("Maximum number of active bullets allowed at once.")]
[SerializeField] private int _maxBulletCount = 10;
```

Rules:
- `[Header]` groups logically related fields. One header per logical group.
- `[Tooltip]` must describe **what the value does** and **its unit or valid range** when relevant.
- Never leave a `[SerializeField]` without a `[Tooltip]`.

---

### 5. XML Documentation — `///` Summary for All Public Members

Every `public` method, property, and class must have a `/// <summary>` block. Private methods only require one if the logic is non-obvious and a name alone is insufficient.

**Full format for methods with parameters:**

```csharp
/// <summary>
/// Reduces the target's HP by the specified amount after applying defense.
/// Triggers <see cref="OnDeath"/> if HP reaches zero.
/// </summary>
/// <param name="rawAmount">Raw incoming damage before defense calculation, must be positive.</param>
/// <param name="damageType">Element type used to apply resistances and weaknesses.</param>
/// <returns>Actual damage dealt after all reductions are applied.</returns>
public float ApplyDamage(float rawAmount, DamageType damageType)
{
    ...
}
```

**Minimal format for simple properties:**

```csharp
/// <summary>Current HP as a value between 0 and <see cref="MaxHp"/>.</summary>
public float CurrentHp => _currentHp;
```

**Rules:**
- `<param>` is required for every parameter — describe its meaning, unit, and constraints.
- `<returns>` is required for every non-void method — describe what is returned and any edge cases (e.g., returns `null` if not found).
- `<exception cref="...">` should be added when the method can throw.
- Reference related members using `<see cref="MemberName"/>`.
- Summary must be written in **third-person present tense**: "Reduces…", "Returns…", "Registers…"

---

### 6. Inline Comments — `//` Only When Code Cannot Self-Explain

Code should explain itself through naming. `//` comments are not a substitute for clear names.

❌ Unnecessary — the code already says this:
```csharp
// Reduce HP
_currentHp -= amount;

// Check if dead
if (_currentHp <= 0) { ... }
```

✅ Required — explains non-obvious reasoning:
```csharp
// Clamp before event dispatch; listeners may read CurrentHp during the callback.
_currentHp = Mathf.Max(0f, _currentHp - amount);
```

✅ Required — explains a deliberate workaround or Unity quirk:
```csharp
// Physics.OverlapSphereNonAlloc returns unordered results.
// Sort is done here once per query rather than per-consumer.
Array.Sort(_hitBuffer, 0, hitCount, _distanceComparer);
```

**Banned comment patterns:**
- `// TODO` without a ticket/issue number
- Commented-out code — use version control instead
- `// end of region` or closing-brace labels
- Restating what the next line does

---

### 7. `#region` — Required for Readability, Forbidden for Hiding

Regions are **mandatory** for enforcing member order and making large scripts navigable. They are **forbidden** as a way to hide poorly structured code.

**Standard region names (use exactly these):**

```csharp
#region Constants
#region Serialized Fields
#region Properties
#region Fields
#region Unity Messages
#region Public Methods
#region Private Methods
#region Coroutines
#region Event Handlers
```

**Rules:**
- Empty regions must be omitted entirely.
- Never nest regions.
- Region names must match the standard list above — no creative variants.
- Regions must follow the member order defined in Rule 2.

**Example structure:**

```csharp
public class PlayerController : MonoBehaviour
{
    #region Constants
    private const float COYOTE_TIME_SEC = 0.12f;
    #endregion

    #region Serialized Fields
    [Header("Movement")]
    [Tooltip("Horizontal speed in units per second.")]
    [SerializeField] private float _moveSpeedUps = 6f;
    #endregion

    #region Fields
    private Rigidbody2D _rigidbody;
    private bool _isGrounded;
    #endregion

    #region Unity Messages
    private void Awake() { ... }
    private void Update() { ... }
    #endregion

    #region Public Methods
    /// <summary>Teleports the player to the specified world position.</summary>
    /// <param name="worldPosition">Target position in world space.</param>
    public void TeleportTo(Vector3 worldPosition) { ... }
    #endregion

    #region Private Methods
    private void ProcessMovementInput() { ... }
    #endregion

    #region Coroutines
    private IEnumerator RespawnRoutine() { ... }
    #endregion

    #region Event Handlers
    private void OnPlayerDeath() { ... }
    #endregion
}
```

---

### 8. `var`, `readonly`, and Type Clarity

**`var`** — only when the right-hand side makes the type unambiguous at a glance:

```csharp
// ✅ Type is obvious
var position = new Vector3(0, 1, 0);
var enemies = new List<Enemy>();

// ❌ Type is not obvious without knowing the return signature
var result = GetProcessedData();
var config = LoadSettings();
```

**`readonly`** — required for any field whose reference must not change after initialization:

```csharp
private readonly List<Enemy> _activeEnemies = new();
private readonly Dictionary<int, ItemData> _itemLookup = new();
```

**`const`** — required for all compile-time literals with domain meaning (see Variable Rules, Rule 8).

---

### 9. Nesting — Maximum 3 Levels, Use Guard Clauses

Deep nesting hides the happy path. Invert conditions to exit early.

❌ Deep nesting — hard to follow:
```csharp
void ProcessHit(Enemy enemy)
{
    if (enemy != null)
    {
        if (enemy.IsAlive)
        {
            if (_canAttack)
            {
                enemy.ApplyDamage(_attackDamage);
            }
        }
    }
}
```

✅ Guard clauses — clear happy path:
```csharp
void ProcessHit(Enemy enemy)
{
    if (enemy == null || !enemy.IsAlive || !_canAttack) return;
    enemy.ApplyDamage(_attackDamage);
}
```

---

### 10. One Class Per File — Strict

Each file contains exactly one primary class, named identically to the file. Exceptions allowed only for:
- Small `struct` types that are tightly coupled to the primary class (e.g., a `DamageData` struct used only by `CombatSystem`)
- `enum` types scoped exclusively to the file's class

Nested classes are forbidden unless they are private implementation details with no external use.

---

### 11. Unity 6.3 — Properties and Access Control

- Never expose fields as `public`. Use properties with restricted setters.
- Prefer `{ get; private set; }` for read-mostly state.
- Use `[field: SerializeField]` for auto-properties that need inspector exposure.

```csharp
// ✅ Encapsulated with inspector support
[field: SerializeField]
public float MaxHp { get; private set; } = 100f;

// ✅ Runtime-only property
public bool IsAlive => _currentHp > 0f;

// ❌ Exposed field — forbidden
public float maxHp = 100f;
```

---

These rules apply uniformly across all `.cs` files in a Unity 6.3 project. When a rule conflicts with a third-party SDK's generated code, isolate the generated file with a `// <auto-generated>` header and exempt it from these rules explicitly.