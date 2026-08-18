
---

## Function Naming & Design Rules
### For Coder Agent — OOP / Unity 6.3 / Performance-Optimized

### Core Philosophy
A function is a **contract**: its name is the promise, its parameters are the inputs, and its return value is the guarantee. A well-designed function can be understood, tested, and replaced without reading its body.

---

### 1. Single Responsibility Principle (SRP) — One Function, One Job

Every function must do **exactly one thing** and do it completely. If you need the word "and" to describe what a function does, it must be split.

❌ `ProcessInputAndUpdateUI()` — two responsibilities
✅ `ProcessInput()` + `RefreshUI()`

A function's job must fit in one sentence without conjunctions. If it cannot, decompose it.

---

### 2. Names Must Start With a Verb That Signals Intent

The verb must communicate **what action** is performed, not how.

| Verb Category | Examples |
|---|---|
| Query (no side effects) | `Get`, `Find`, `Calculate`, `Resolve`, `Check` |
| Command (has side effects) | `Apply`, `Save`, `Send`, `Spawn`, `Destroy`, `Register` |
| Predicate (returns bool) | `Is`, `Has`, `Can`, `Should`, `Validate` |
| Conversion | `To`, `From`, `Convert`, `Parse`, `Serialize` |
| Lifecycle | `Initialize`, `Dispose`, `Reset`, `Rebuild` |

✅ `CalculateFinalDamage()`, `ApplyStatusEffect()`, `IsTargetInRange()`
❌ `DamageStuff()`, `DoThing()`, `Handle()`, `Process()`

---

### 3. Casing Convention by Function Type

| Context | Convention | Example |
|---|---|---|
| Public methods (C# Unity) | `PascalCase` | `ApplyDamage()` |
| Private methods (C# Unity) | `PascalCase` or `_camelCase` | `_recalculateStats()` |
| Coroutines | `PascalCase` + noun suffix | `FadeOutRoutine()` |
| Predicates | `PascalCase` verb + subject | `IsEnemyVisible()` |
| Event callbacks | `On` prefix + event name | `OnPlayerDeath()` |
| Lambda / local functions | `camelCase` | `var computeScore = () => ...` |

---

### 4. Command vs. Query Separation (CQS)

Functions must be either a **Command** (does something, returns `void`) or a **Query** (returns a value, no side effects) — never both.

✅ Query — no mutation:
```csharp
float GetNormalizedHealth() => currentHp / maxHp;
```

✅ Command — explicit side effect in name:
```csharp
void ApplyDamage(float amount) { ... }
```

❌ Mixed — forbidden pattern:
```csharp
float ApplyDamageAndReturnRemaining(float amount) { ... }
```

If a value must be returned after a mutation, use an `out` parameter or return a result object — but keep the command and query logic in separate methods.

---

### 5. Functions With Side Effects Must Declare It in the Name

Any function that mutates state, writes to disk, sends a network call, or modifies a scene object must use a verb that signals mutation.

✅ `SavePlayerProgress()`, `BroadcastEvent()`, `DestroyProjectile()`, `UpdateLeaderboard()`
❌ `PlayerProgress()`, `Event()`, `Projectile()` ← verbs are not optional

Side-effect-free functions (pure functions) should be preferred for utilities, calculations, and predicates. Mark them with `// pure` comment if helpful for reviewers.

---

### 6. OOP-Friendly Parameter Design — Pass Data, Don't Reach

Functions must **never access scene objects or globals internally** when that data can be passed as a parameter. This makes functions testable and decoupled.

❌ Reaches into scene — tightly coupled:
```csharp
void ApplyDamage() {
    var player = GameObject.Find("Player"); // forbidden inside functions
    player.GetComponent<Health>().hp -= damage;
}
```

✅ Data-driven — OOP-friendly:
```csharp
void ApplyDamage(Health target, float amount) {
    target.Reduce(amount);
}
```

Dependencies required at class level must be injected via constructor, `[SerializeField]`, or `Awake()` — never fetched inside methods.

---

### 7. Unity 6.3 Lifecycle Rules — Keep Lifecycle Functions Thin

Unity lifecycle methods (`Awake`, `Start`, `Update`, `FixedUpdate`, `LateUpdate`, `OnEnable`, `OnDisable`, `OnDestroy`) must be **coordinators only** — they call other functions, they do not contain logic themselves.

```csharp
// ✅ Correct — lifecycle as coordinator
void Update() {
    ProcessMovementInput();
    UpdateAnimationState();
}

// ❌ Wrong — logic buried in lifecycle
void Update() {
    if (Input.GetKey(KeyCode.W)) {
        transform.position += Vector3.forward * speed * Time.deltaTime;
        animator.SetBool("isWalking", true);
    }
}
```

**Unity 6.3 specific:** Prefer `IUpdateSystem` interfaces and ECS-compatible update patterns over monolithic `Update()` methods. Use `[RuntimeInitializeOnLoadMethod]` for initialization logic that doesn't belong in a scene object.

---

### 8. Performance — Non-Alloc & Allocation-Aware Functions

Any function called frequently (every frame, on physics tick, or in hot loops) must be written to produce **zero heap allocations**.

**Naming signals for optimized functions:**

| Suffix/Prefix | Meaning | Example |
|---|---|---|
| `NonAlloc` | Uses pre-allocated buffer | `FindTargetsNonAlloc()` |
| `Cached` | Returns or uses a cached result | `GetCachedNavPath()` |
| `Burst` | Intended for Unity Burst compilation | `ComputeWeightsBurst()` |
| `Batch` | Processes multiple items in one call | `UpdateTransformsBatch()` |

**Forbidden inside functions called per-frame:**
- `new`, object/array allocation
- `GameObject.Find()`, `FindObjectOfType()`
- `GetComponent<T>()` (cache in `Awake` instead)
- LINQ (`.Where()`, `.Select()`, `.FirstOrDefault()`)
- `string` concatenation (use `StringBuilder` or `string.Format`)
- `Instantiate()` / `Destroy()` (use Object Pooling)

```csharp
// ❌ Allocates every frame
void Update() {
    var enemies = FindObjectsOfType<Enemy>(); // allocation
    var alive = enemies.Where(e => e.IsAlive).ToList(); // LINQ + allocation
}

// ✅ Zero-alloc equivalent
private readonly Collider[] _hitBuffer = new Collider[32]; // pre-allocated

void Update() {
    int count = Physics.OverlapSphereNonAlloc(transform.position, _radius, _hitBuffer);
    ProcessHitsNonAlloc(_hitBuffer, count);
}
```

---

### 9. Coroutine Design Rules

Coroutines must follow strict naming and structural conventions.

| Role | Convention | Example |
|---|---|---|
| The coroutine IEnumerator | `PascalCase` + `Routine` suffix | `FadeOutRoutine()` |
| Public starter method | `Start` + concept name | `StartFadeOut()` |
| Public stopper method | `Stop` + concept name | `StopFadeOut()` |
| Stored reference | `_` + concept + `Coroutine` | `_fadeCoroutine` |

```csharp
private Coroutine _fadeCoroutine;

public void StartFadeOut() {
    _fadeCoroutine = StartCoroutine(FadeOutRoutine());
}

public void StopFadeOut() {
    if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
}

private IEnumerator FadeOutRoutine() { ... }
```

Never call `StartCoroutine()` from inside `Update()` without a guard — it will spawn a new coroutine every frame.

---

### 10. Event Callback Naming

Event-receiving functions must use `On` prefix. The name must identify the **source** and the **event**, not the response action.

✅ `OnPlayerDeath()`, `OnEnemyEnteredRange()`, `OnInventoryChanged()`
❌ `HandleDeath()`, `EnemyDetected()`, `InventoryUpdate()`

When subscribing and unsubscribing, always pair them in `OnEnable`/`OnDisable`:

```csharp
void OnEnable()  => EventBus.PlayerDied += OnPlayerDeath;
void OnDisable() => EventBus.PlayerDied -= OnPlayerDeath;
```

---

### 11. Function Length and Decomposition

| Lines | Status |
|---|---|
| 1–15 | Ideal |
| 16–30 | Acceptable with justification |
| 31–50 | Must be split unless it is a single flat switch/match |
| 50+ | Always forbidden — refactor immediately |

When splitting, extract by **semantic step**, not by line count. Each extracted function must be independently named and meaningful.

---

### 12. Avoid Overloads — Prefer Explicit Names

Overloads hide intent and complicate call sites. Use explicit names instead.

❌ Overloads that obscure meaning:
```csharp
void Attack() { }
void Attack(float multiplier) { }
void Attack(Enemy target) { }
```

✅ Explicit names:
```csharp
void AttackDefault() { }
void AttackWithMultiplier(float multiplier) { }
void AttackTarget(Enemy target) { }
```

Overloads are permitted only when the function is a public API boundary and all variants share **identical semantic intent** with only input format differing (e.g., accepting `Vector2` vs `Vector3`).

---