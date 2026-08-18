# Combat System — Overview & User Manual

## Overview
The `Combat` folder contains the foundational, interface-driven abstractions for all combat interactions in BanRaiValley.
These types are shared across systems (Player, Plant AI, Enemy AI) and define a single, consistent contract for dealing and receiving damage.

No concrete game logic lives here — this folder is a **contract layer only**.

---

## Files

| File | Type | Purpose |
|---|---|---|
| `DamageType.cs` | `enum` | Categorises damage (Physical, Slashing, Blunt, Piercing, Fire, Water). |
| `DamageData.cs` | `readonly struct` | Immutable value object carrying all data for a single damage hit. |
| `IDamageable.cs` | `interface` | Contract for entities that can receive damage. |
| `IAIAgent.cs` | `interface` | Contract for AI agents that acquire/release a combat target. |

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
