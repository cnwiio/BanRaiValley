# Plant AI System — README

**Folder:** `Assets/Project/Scripts/AI/PlantAI/`  
**System:** Plant AI — *The Living Harvest* Monster Pipeline  
**Unity Version:** 6.3  
**Author:** AI Coder Agent  
**Last Updated:** 2026-08-19

---

## Overview

The **Plant AI System** powers the *Living Harvest* core loop of BanRaiValley: mature crops in the farming grid can awaken into hostile plant monsters when a player attempts to harvest them. The player must then defeat the monster to clear the tile and retrieve loot.

The system is composed of **six focused, single-responsibility components** grouped into three architectural layers:

| Layer | Component | Responsibility |
|---|---|---|
| **Data** | `PlantAIConfigSO` | All balance values (speed, damage, cooldowns, timing) |
| **Data** | `PlantLootTableSO` | Loot entries evaluated on monster death |
| **Sensing** | `PlantPerception` | Trigger-zone-based target detection — no Update polling |
| **Health** | `PlantHealth` | HP management, damage processing, death detection |
| **Action** | `PlantMovement` | NavMeshAgent locomotion wrapper with smooth rotation |
| **Action** | `PlantCombat` | Melee attack sequencing via tracked coroutines |
| **Action** | `PlantAnimationController` | Pre-hashed Animator parameter access |
| **Brain** | `PlantBrain` | Central FSM coordinator implementing `IAIAgent` |
| **Integration** | `AwakenedCropHarvestTrigger` | Converts a static mature crop into a live monster |
| **Integration** | `PlantSpawner` | Pools monster instances and clears farm tiles on death |

---

## Architecture & Design Principles

### Zero-Update-Polling — Event & Timer Driven

**No AI decision logic is ever polled inside `Update`.** All state changes are triggered by:

- **Unity Trigger Callbacks** — `PlantPerception` uses `OnTriggerEnter` / `OnTriggerExit` for target detection.
- **Local C# Events** — `PlantHealth.OnDamaged`, `PlantHealth.OnDied`, `PlantPerception.OnTargetDetected`, etc.
- **Timed Coroutines** — `PlantBrain.ChaseTargetRoutine` updates the destination every `0.25 s` (not per frame).

All coroutine references are stored in named private fields and explicitly stopped in `OnDisable` to prevent memory leaks.

### State Machine Flow

```
Dormant ──[Awaken()]──► Awakening ──[AwakeningDurationSec]──► Idle
                                                               │
                                          OnTargetDetected ◄──┘
                                               │
                                               ▼
                                             Chase ◄──────────────┐
                                               │                  │
                                  within AttackRangeM             │
                                               │                  │
                                               ▼                  │
                                            Attack ──[OnAttackCompleted]──►HasTarget?
                                               │
                                          [OnDamaged]
                                               │
                                               ▼
                                           HitReact ──[HitStaggerDurationSec]──► Chase/Idle
                                               │
                                          [OnDied()]
                                               │
                                               ▼
                                             Dead ──[DespawnDelay]──► LeanPool.Despawn
```

Every state transition raises `EventBus<OnPlantStateChangedEvent>` so external systems (UI, audio, quests) can react without coupling.

### Interface Contracts

| Interface | File | Purpose |
|---|---|---|
| `IDamageable` | `Combat/IDamageable.cs` | Any entity that can receive `DamageData` hits |
| `IAIAgent` | `Combat/IAIAgent.cs` | Provides `TargetTransform`, `CurrentState`, `SetTarget`, `ClearTarget` |
| `IFarmingGrid` | `Farming/Gird/IFarmingGrid.cs` | Decouples farm-grid operations from concrete `FarmingGrid` |

All component-to-component coupling uses interface queries (`GetComponent<IDamageable>()`) — never concrete type casts.

### Composition over Inheritance

`PlantBrain` **has** a `PlantHealth`, `PlantPerception`, `PlantMovement`, `PlantCombat`, and `PlantAnimationController` — it does not inherit from any of them. Max inheritance depth is `MonoBehaviour → PlantBrain` (depth 1).

---

## Inspector Setup & User Manual

### Step 1 — Create a PlantAIConfigSO Asset

1. In the **Project** window, right-click → **Create → BanRaiValley → AI → Plant AI Config**.
2. Name it after the plant variant (e.g. `Config_TomatoMonster`).
3. Configure the fields:

| Field | Description | Recommended Defaults |
|---|---|---|
| `MaxHp` | Hit points | 50 |
| `HitStaggerDurationSec` | Flinch duration | 0.3 s |
| `MoveSpeedUps` | NavMesh speed (units/s) | 3.5 |
| `RotationSpeedDeg` | Smooth-turn speed (°/s) | 360 |
| `AggroRadiusM` | Sphere trigger radius (m) | 8 |
| `AttackRangeM` | Max melee distance (m) | 1.8 |
| `StoppingDistanceM` | NavMesh stop distance (m) | 1.2 |
| `BaseAttackDamage` | Raw damage per hit | 10 |
| `AttackCooldownSec` | Min time between attacks | 1.5 s |
| `AttackWindupSec` | Delay before hitbox opens | 0.4 s |
| `AwakeningDurationSec` | Uprooting animation length | 1.2 s |
| `AwakeningTriggerName` | Animator trigger name | `"Awaken"` |
| `AttackTriggerName` | Animator trigger name | `"Attack"` |
| `HitTriggerName` | Animator trigger name | `"Hit"` |
| `DieTriggerName` | Animator trigger name | `"Die"` |

### Step 2 — Create a PlantLootTableSO Asset

1. Right-click → **Create → BanRaiValley → AI → Plant Loot Table**.
2. Add entries to **Loot Drops**:
   - `Item` — drag the `Item` ScriptableObject reference.
   - `MinQuantity` / `MaxQuantity` — drop quantity range.
   - `DropChancePercent` — probability (0–100 %).

### Step 3 — Build the Monster Prefab

Create a new prefab (e.g. `Monster_TomatoPlant.prefab`) and attach the following components **in this order**:

#### Root GameObject
| Component | Notes |
|---|---|
| `PlantBrain` | Wire all subsystems in the inspector |
| `PlantHealth` | No inspector fields needed — initialized by `PlantBrain.Awake` |
| `PlantPerception` | Assign `_aggroTrigger` and `_targetLayer` |
| `PlantMovement` | Assign `_navMeshAgent` (auto-found via `RequireComponent`) |
| `PlantCombat` | Assign `_meleeHitbox` and `_projectileSpawnPoint` |
| `PlantAnimationController` | Assign `_animator` and verify trigger name strings match `PlantAIConfigSO` |
| `NavMeshAgent` | Required by `PlantMovement` |
| `Collider(s)` | Add body colliders and reference them in `PlantBrain._bodyColliders` |

#### Child GameObject — Aggro Trigger
1. Add a child object named `AggroZone`.
2. Add a `SphereCollider` → set **Is Trigger = true**.
3. Assign this collider to `PlantPerception._aggroTrigger`.
4. The radius is overridden at runtime by `PlantPerception.Initialize(aggroRadiusM)`.

#### Child GameObject — Melee Hitbox
1. Add a child object named `MeleeHitbox`.
2. Add a `Collider` (Box or Capsule) → set **Is Trigger = true**, **disable it by default**.
3. Add `PlantMeleeHitbox` component → assign `_hitboxCollider`.
4. Assign this `PlantMeleeHitbox` to `PlantCombat._meleeHitbox`.

### Step 4 — Wire PlantBrain in Inspector

With the prefab open, select the root and fill `PlantBrain` serialized fields:

- **Configuration** — drag `PlantAIConfigSO` and `PlantLootTableSO`.
- **Internal Subsystems** — drag each sibling component (`PlantHealth`, `PlantPerception`, `PlantMovement`, `PlantCombat`, `PlantAnimationController`).
- **Body Colliders** — add all non-trigger body colliders that should be disabled on death.
- **Despawn Delay Sec** — leave at `4 s` unless the death animation is longer.

### Step 5 — Configure Layers for Player Detection

1. Create a Unity Layer named `Player` (or reuse an existing one).
2. Set the **player character's** GameObject to that layer.
3. In `PlantPerception._targetLayer`, select only the **Player** layer.
4. Ensure the aggro trigger sphere does **not** have any conflicting layer masks in Physics settings.

### Step 6 — Set Up AwakenedCropHarvestTrigger

1. Add `AwakenedCropHarvestTrigger` to the **mature crop prefab** (or its interaction detector object).
2. Assign `_awakenedMonsterPrefab` → the monster prefab built in Steps 3–4.
3. Ensure the monster prefab is registered with **LeanPool** (`LeanGameObjectPool` on a scene manager or the prefab itself).
4. Call `harvestTrigger.Initialize(cellPos)` from the crop planting/maturation system when the crop reaches full growth.
5. Call `harvestTrigger.TriggerAwakening()` from the player's harvest interaction code.

### Step 7 — Add PlantSpawner to the Scene

1. Create an empty scene GameObject named `PlantSpawner`.
2. Attach the `PlantSpawner` component.
3. Set `_poolReturnDelaySec` to be slightly longer than `PlantBrain._despawnDelaySec` (default `4.5 s`).
4. No further wiring needed — `PlantSpawner` subscribes to `EventBus<OnPlantDiedEvent>` automatically in `OnEnable`.

---

## Event Reference

All events flow through the generic `EventBus<T>` static bus. Subscribe / unsubscribe in `OnEnable` / `OnDisable`.

| Event Struct | When Raised | Key Fields |
|---|---|---|
| `OnPlantAwakenedEvent` | Crop transforms into monster | `PlantInstance`, `CellPos`, `WorldPosition` |
| `OnPlantStateChangedEvent` | Any FSM state transition | `PlantInstance`, `PreviousState`, `NewState` |
| `OnPlantDamagedEvent` | Plant receives valid damage | `PlantInstance`, `DamageData`, `CurrentHp`, `MaxHp` |
| `OnPlantDiedEvent` | Plant HP reaches zero | `PlantInstance`, `Position`, `CellPos` |
| `OnPlantAttackExecutedEvent` | Plant completes an attack | `PlantInstance`, `TargetTransform`, `DamageAmount` |
| `OnClearPlant` | Static crop or dead monster removed | `CellPos` |

### Usage Example

```csharp
// Subscribe in OnEnable — unsubscribe in OnDisable
private void OnEnable()
{
    EventBus<OnPlantDiedEvent>.Subscribe(HandlePlantDied);
}

private void OnDisable()
{
    EventBus<OnPlantDiedEvent>.Unsubscribe(HandlePlantDied);
}

private void HandlePlantDied(OnPlantDiedEvent evt)
{
    Debug.Log($"Plant at {evt.Position} died. Cell: {evt.CellPos}");
}
```

---

## File Inventory

| File | Type | Description |
|---|---|---|
| `PlantAIConfigSO.cs` | ScriptableObject | All balance data for one plant variant |
| `PlantLootTableSO.cs` | ScriptableObject | Loot drop table evaluated on death |
| `PlantAIState.cs` | Enum | 7 discrete FSM states |
| `PlantHealth.cs` | MonoBehaviour | HP + damage + death, implements `IDamageable` |
| `PlantPerception.cs` | MonoBehaviour | Trigger-based aggro detection |
| `PlantMovement.cs` | MonoBehaviour | NavMeshAgent locomotion wrapper |
| `PlantMeleeHitbox.cs` | MonoBehaviour | Trigger-driven melee hit registration |
| `PlantCombat.cs` | MonoBehaviour | Attack coroutine pipeline + cooldown |
| `PlantAnimationController.cs` | MonoBehaviour | Pre-hashed Animator parameter access |
| `PlantBrain.cs` | MonoBehaviour | Central FSM brain, implements `IAIAgent` |
| `AwakenedCropHarvestTrigger.cs` | MonoBehaviour | Crop-to-monster transformation trigger |
| `PlantSpawner.cs` | MonoBehaviour | LeanPool return + tile-clear on monster death |
| `README.md` | Documentation | This file |
