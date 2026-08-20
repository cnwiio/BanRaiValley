# Combat System — Overview & User Manual

## Overview
The `Combat` folder contains the foundational, interface-driven abstractions for all combat interactions in BanRaiValley.
These types are shared across systems (Player, Plant AI, Enemy AI) and define a single, consistent contract for dealing and receiving damage.

---

## Files

| File | Type | Purpose |
|---|---|---|
| `DamageType.cs` | `enum` | Categorises damage (Physical, Slashing, Blunt, Piercing, Fire, Water). |
| `DamageData.cs` | `readonly struct` | Immutable value object carrying all data for a single damage hit. |
| `IDamageable.cs` | `interface` | Contract for entities that can receive damage. |
| `IAIAgent.cs` | `interface` | Contract for AI agents that acquire/release a combat target. |
| `PlayerCombatController.cs` | `MonoBehaviour` | Manages the player's melee attack loop: cooldown → animation → hit detection → damage. |

---

## User Manual

### Dealing Damage
1. Build a `DamageData` struct with the required parameters:
```csharp
var hit = new DamageData(
    amount:         25f,
    type:           DamageType.Slashing,
    source:         gameObject,
    hitPoint:       contactPoint.point,
    hitNormal:      contactPoint.normal,
    knockbackForce: 5f
);
```

2. Obtain the `IDamageable` interface from the target — **never cast to a concrete type**:
```csharp
if (other.TryGetComponent<IDamageable>(out var damageable))
    damageable.TakeDamage(hit);
```

### Implementing IDamageable
```csharp
public class PlantHealthComponent : MonoBehaviour, IDamageable
{
    [SerializeField] private float _maxHp = 100f;
    private float _currentHp;

    public float CurrentHp => _currentHp;
    public float MaxHp     => _maxHp;
    public bool  IsAlive   => _currentHp > 0f;

    private void Awake() => _currentHp = _maxHp;

    public void TakeDamage(DamageData damageData)
    {
        _currentHp = Mathf.Max(0f, _currentHp - damageData.Amount);
        EventBus<OnPlantDamagedEvent>.Raise(new OnPlantDamagedEvent
        {
            PlantInstance = gameObject,
            DamageData    = damageData,
            CurrentHp     = _currentHp,
            MaxHp         = _maxHp,
        });
        if (!IsAlive) HandleDeath();
    }
}
```

### Implementing IAIAgent
Implement `IAIAgent` on your AI controller (or a dedicated TargetingComponent).
Call `SetTarget` from the Perception layer, and `ClearTarget` when the target is lost.
State changes must be emitted via `OnPlantStateChangedEvent` on the EventBus — never polled.

---

## Rules
- One class/interface per file.
- All `IDamageable` consumers use `GetComponent<IDamageable>()` — no `is`/`as` casts.
- `DamageData` is immutable (`readonly struct`) — never mutate fields mid-flight.
- Subscribe to Plant AI events in `OnEnable`, unsubscribe in `OnDisable`.

---

---

# Player Attack System — Overview & User Manual

## Architecture Overview

The Player Attack System is a fully event-driven, Minecraft-style melee combat system built on four cooperating components. No component polls another directly — all coordination flows through `EventBus<T>`.

```
[Input System]
      │
      ▼ OnPrimaryActionEvent
[FarmingToolBase / Hoe]          [PlayerCombatController]
 ├─ Idle  → OnPlayerRequestAttackEvent ──────────────────────────────┐
 ├─ Farming → StartTilling()                                         │
 └─ Deleting → DeletePlant/Tile()                                    │
                                                                      ▼
                                                           RequestAttack()
                                                            ├─ cooldown gate
                                                            ├─ ResolveAttackData()
                                                            ├─ TriggerAttackAnimation()
                                                            ├─ Raise OnPlayerAttackExecutedEvent
                                                            └─ Coroutine: DelayedHitDetection()
                                                                  └─ Physics.OverlapBoxNonAlloc()
                                                                        └─ ProcessHit() × N
                                                                              ├─ IDamageable.TakeDamage()
                                                                              └─ Raise OnPlayerHitTargetEvent
[PlayerHandVisualizer]
 ├─ Empty slot → Spawn _bareHandPrefab (LeanPool)
 ├─ Filled slot → Spawn item.prefab (LeanPool)
 └─ TriggerAttackAnimation() → Animator.SetTrigger("Attack")
```

### Component Responsibilities

| Component | Responsibility |
|---|---|
| `ItemAttackData` | Pure data: damage, cooldown, stamina cost, box geometry. Serialized on `Item` SO. |
| `PlayerHandVisualizer` | Spawns/despawns hand/item models. Provides `TriggerAttackAnimation()`. |
| `PlayerCombatController` | Cooldown gate, animation trigger, OverlapBox detection, damage dispatch. |
| `Hoe.PrimaryAction()` | Context router: Idle → attack event, Farming/Deleting → farming actions. |

---

## EventBus Events

| Event Struct | Raised By | When | Payload |
|---|---|---|---|
| `OnPlayerRequestAttackEvent` | `Hoe` (Idle), external systems | Player inputs an attack intent | *(empty)* |
| `OnPlayerAttackExecutedEvent` | `PlayerCombatController` | Attack animation + OverlapBox fired | `EquippedItem`, `AttackData`, `AttackOrigin` |
| `OnPlayerHitTargetEvent` | `PlayerCombatController` | A valid `IDamageable` was hit | `TargetInstance`, `DamageData`, `TargetDied` |

### Subscribing to Player Hit Events
```csharp
private void OnEnable()
{
    EventBus<OnPlayerHitTargetEvent>.Subscribe(OnPlayerHitTarget);
}

private void OnDisable()
{
    EventBus<OnPlayerHitTargetEvent>.Unsubscribe(OnPlayerHitTarget);
}

private void OnPlayerHitTarget(OnPlayerHitTargetEvent evt)
{
    Debug.Log($"Hit {evt.TargetInstance.name} for {evt.DamageData.Amount} dmg. Died: {evt.TargetDied}");
}
```

---

## Designer Setup Guide

### 1. Configuring `ItemAttackData` on an Item ScriptableObject

Every `Item` ScriptableObject exposes a **Combat** header in the Inspector with an `_attackData` field of type `ItemAttackData`.

| Field | Description | Default (Unarmed) |
|---|---|---|
| `canAttack` | Enables/disables attacking with this item. Set `false` for Seeds, Planters, etc. | `true` |
| `damageAmount` | Raw damage per hit (before resistances). | `2` |
| `damageType` | `Physical`, `Slashing`, `Blunt`, `Piercing`, `Fire`, `Water`. | `Physical` |
| `attackCooldownSec` | Minimum seconds between consecutive swings. | `0.4` |
| `staminaCost` | Stamina deducted per swing (wired by stamina system). | `1` |
| `knockbackForce` | Impulse applied to the target's Rigidbody. | `2` |
| `attackBoxSize` | XYZ half-extents of the OverlapBox hitbox. | `(1.2, 1.2, 2.0)` |
| `attackBoxOffset` | Local offset from camera origin where the box is centred. | `(0, 0, 1.2)` |

**Step-by-step:**
1. Select any `Item` ScriptableObject in the Project window.
2. Scroll to the **Combat** header.
3. Check `canAttack` to allow swinging.
4. Tune `damageAmount`, `damageType`, and `attackCooldownSec` for balance.
5. Adjust `attackBoxSize` and `attackBoxOffset` to match the weapon's visual reach (use the Scene Gizmo on `PlayerCombatController` to preview).

> **Non-attackable items** (e.g. `SeedBag`): Uncheck `canAttack`. `PlayerCombatController` will silently reject the swing.

---

### 2. Setting Up the Bare Hand on `PlayerHandVisualizer`

`PlayerHandVisualizer` spawns a fallback model when the active hotbar slot is empty.

**Step-by-step:**
1. Select the **Player** GameObject in the Hierarchy.
2. Find the `PlayerHandVisualizer` component.
3. Assign a Bare Hand prefab to the **Bare Hand → `_bareHandPrefab`** slot.
   - The prefab should be **LeanPool-compatible** (registered in `LeanPool` or a plain `GameObject`).
   - For attack animations, add an `Animator` with an **"Attack"** trigger parameter anywhere in the prefab hierarchy.
4. The `_spawnTransform` should point to the first-person hand bone/socket.

> If `_bareHandPrefab` is left empty, the hand slot simply stays invisible when unarmed — no errors are thrown.

---

### 3. Configuring the Attack OverlapBox in `PlayerCombatController`

`PlayerCombatController` manages cooldowns, animations, and hit detection. Attach it to the **Player** root.

**Inspector fields:**

| Field | Description |
|---|---|
| `_handVisualizer` | Reference to `PlayerHandVisualizer` on the Player. |
| `_attackCameraTransform` | Transform used as the attack origin (auto-resolves to `Camera.main` if left empty). |
| `_hitLayers` | LayerMask — only colliders on these layers are considered valid hits. |
| `_unarmedAttackData` | Fallback `ItemAttackData` used when no item is equipped. |
| `_impactDelaySec` | Delay (seconds) between animation trigger and OverlapBox check. Tune to match the swing animation peak. |
| `_maxTargetCount` | Maximum number of targets the OverlapBox can register per swing (pre-allocated buffer). |

**Step-by-step:**
1. Add `PlayerCombatController` to the **Player** root GameObject.
2. Drag `PlayerHandVisualizer` into `_handVisualizer`.
3. Leave `_attackCameraTransform` empty (auto-resolves from `Camera.main`) or assign the FPS camera explicitly.
4. Set `_hitLayers` to include all layers that enemies occupy (e.g., `Enemy`, `PlantAI`).
5. Select the Player in the Scene and look at the **Scene View** — an orange wireframe box shows the current attack volume. Adjust `attackBoxSize` and `attackBoxOffset` on the equipped `Item`'s `ItemAttackData` until the box aligns with the weapon tip.
6. Tune `_impactDelaySec` to synchronise the hit moment with the swing animation keyframe.

---

### 4. Context-Aware Attack with the Hoe

The `Hoe` tool routes `PrimaryAction()` based on its current state:

| `HoeState` | Left-Click Behaviour |
|---|---|
| `Idle` | Raises `OnPlayerRequestAttackEvent` → `PlayerCombatController` performs a melee swing. |
| `Farming` | Tills the targeted soil cell (no attack). |
| `Deleting` | Removes plant or tilled tile (no attack). |

**Workflow:** Right-click toggles `Farming` mode. Left-click in `Idle` swings the Hoe as a weapon.

---

## Performance Notes

- `Physics.OverlapBoxNonAlloc` with a pre-allocated `Collider[]` buffer — **zero GC allocations** per swing.
- `LeanPool.Spawn` / `LeanPool.Despawn` used for all hand/item model lifecycle — no instantiation GC spikes.
- No polling in `Update` — all input and state changes are driven by EventBus events.
- Pending hit-check coroutines are cancelled in `OnDisable` — no dangling callbacks after the component deactivates.
