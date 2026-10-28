# Task 01: Create Abstract Base Classes for AI Components

## 1. Task Goal
Create clean, decoupled abstract base classes for all modular AI components (`AIMovementBase`, `AIAttackBase`, `AIDetectionBase`, `AIAnimationControllerBase`, `AIHealthBase`) adhering to the project's architecture rules (Composition over inheritance, interface/abstract contracts, zero leaks, XML documentation, `#region` structures).

## 2. Task Information
- **System**: Plant AI System
- **Parent Plan**: [plant-ai-framework-plan.md](../../plan/plant-ai-framework-plan.md)
- **Target File(s)**:
  - `Assets/Project/Scripts/AI/AIMovementBase.cs`
  - `Assets/Project/Scripts/AI/AIAttackBase.cs`
  - `Assets/Project/Scripts/AI/AIDetectionBase.cs`
  - `Assets/Project/Scripts/AI/AIAnimationControllerBase.cs`
  - `Assets/Project/Scripts/AI/AIHealthBase.cs`
- **Dependencies / Prerequisites**:
  - None
- **Applicable Rules**:
  - `architecture-guide.md`: Section 3 (Composition over inheritance, max depth 2), Section 4 (Interface-driven), Section 10 (AI Perception/Decision/Action).
  - `code-style-rule.md`: `#region` blocks, XML documentation on all public members, Allman brace style, no public fields.
  - `function-rule.md`: Command vs Query separation, PascalCase.
  - `naming-variable-rule.md`: Modal verbs on booleans (`isAttacking`, `isPlayerInSight`).

## 3. What To Do (Step-by-Step Instructions)

### 3.1. `AIMovementBase.cs`
Create `Assets/Project/Scripts/AI/AIMovementBase.cs`:
```csharp
using UnityEngine;

/// <summary>
/// Abstract base class for plant locomotion and rotation.
/// Concrete implementations handle specific navigation strategies (e.g. NavMesh, rooted, hopping).
/// </summary>
public abstract class AIMovementBase : MonoBehaviour
{
    #region Properties
    /// <summary>Gets the current movement speed of the plant.</summary>
    public abstract float Speed { get; }

    /// <summary>Gets whether the plant has reached its current navigation destination.</summary>
    public abstract bool IsReachDestination { get; }

    /// <summary>Gets the target transform being chased or focused.</summary>
    public abstract Transform TargetTransform { get; }
    #endregion

    #region Public Methods
    /// <summary>Initializes movement parameters from configuration data.</summary>
    /// <param name="startPosition">Home / spawn position to return to when losing target.</param>
    /// <param name="data">Configuration data asset containing speed, stopping distance, etc.</param>
    public abstract void Initialize(Vector3 startPosition, AIData data);

    /// <summary>Binds a target transform for chasing and orientation.</summary>
    /// <param name="target">Target transform to pursue.</param>
    public abstract void BindTargetTransform(Transform target);

    /// <summary>Unbinds the current target transform.</summary>
    public abstract void UnBindTargetTransform();

    /// <summary>Commands the movement system to navigate toward a destination coordinate.</summary>
    /// <param name="targetPos">World destination position.</param>
    public abstract void MoveTo(Vector3 targetPos);

    /// <summary>Commands the plant to navigate back to its starting/home position.</summary>
    public abstract void ReturnToStart();

    /// <summary>Stops all active movement immediately.</summary>
    public abstract void StopMoving();

    /// <summary>Rotates the plant towards its currently bound target.</summary>
    public abstract void RotateToTarget();
    #endregion
}
```

### 3.2. `AIAttackBase.cs`
Create `Assets/Project/Scripts/AI/AIAttackBase.cs`:
```csharp
using System;
using UnityEngine;

/// <summary>
/// Abstract base class for plant combat actions.
/// Concrete implementations handle specific attack mechanics (e.g. melee hitbox, ranged projectile, spore explosion).
/// </summary>
public abstract class AIAttackBase : MonoBehaviour
{
    #region Properties
    /// <summary>Gets whether the plant is ready to initiate an attack (cooldown elapsed).</summary>
    public abstract bool CanAttack { get; }

    /// <summary>Gets whether an attack is actively in progress.</summary>
    public abstract bool IsAttacking { get; }
    #endregion

    #region Events
    /// <summary>Event raised when the attack sequence completes its recovery/cooldown.</summary>
    public abstract event Action OnAttackEndEvent;
    #endregion

    #region Public Methods
    /// <summary>Initializes attack parameters from configuration data.</summary>
    /// <param name="data">Configuration data asset containing damage, cooldown, etc.</param>
    public abstract void Initialize(AIData data);

    /// <summary>Prepares the plant to attack, consuming availability until cooldown completes.</summary>
    public abstract void StartAttack();

    /// <summary>Executes the actual damage delivery (triggered via animation hit event).</summary>
    public abstract void ExecuteAttack();

    /// <summary>Interrupts and cancels any active attack or cooldown sequence immediately.</summary>
    public abstract void StopAttack();
    #endregion
}
```

### 3.3. `AIDetectionBase.cs`
Create `Assets/Project/Scripts/AI/AIDetectionBase.cs`:
```csharp
using System;
using UnityEngine;

/// <summary>
/// Abstract base class for plant perception and target detection.
/// Concrete implementations handle target sensing (e.g. sphere trigger, line-of-sight raycast).
/// </summary>
public abstract class AIDetectionBase : MonoBehaviour
{
    #region Properties
    /// <summary>Gets whether a valid player target is currently in perception range.</summary>
    public abstract bool IsPlayerInSight { get; }

    /// <summary>Gets the transform of the currently spotted target, if any.</summary>
    public abstract Transform TargetTransform { get; }
    #endregion

    #region Events
    /// <summary>Event raised when a target enters detection range.</summary>
    public abstract event Action<Transform> OnTargetDetectedEvent;

    /// <summary>Event raised when the target leaves detection range.</summary>
    public abstract event Action OnTargetLostEvent;
    #endregion

    #region Public Methods
    /// <summary>Initializes detection parameters.</summary>
    /// <param name="detectionRange">Radius or range of target perception.</param>
    public abstract void Initialize(float detectionRange);

    /// <summary>Enables or disables target sensing.</summary>
    /// <param name="isEnabled">True to enable detection; false to turn off perception.</param>
    public abstract void EnableDetect(bool isEnabled);
    #endregion
}
```

### 3.4. `AIAnimationControllerBase.cs`
Create `Assets/Project/Scripts/AI/AIAnimationControllerBase.cs`:
```csharp
using UnityEngine;

/// <summary>
/// Abstract base class for plant animation triggering.
/// </summary>
public abstract class AIAnimationControllerBase : MonoBehaviour
{
    #region Public Methods
    /// <summary>Fires the awaken/uprooting animation trigger.</summary>
    public abstract void PlayAwaken();

    /// <summary>Fires the attack animation trigger.</summary>
    public abstract void PlayAttack();

    /// <summary>Fires the hit-react / flinch animation trigger.</summary>
    public abstract void PlayHit();

    /// <summary>Fires the death animation trigger.</summary>
    public abstract void PlayDie();

    /// <summary>Updates locomotion animation speed parameter.</summary>
    /// <param name="speed">Current movement speed magnitude.</param>
    public abstract void SetMoving(float speed);

    /// <summary>Resets the animation state machine to default.</summary>
    public abstract void Reset();
    #endregion
}
```

### 3.5. `AIHealthBase.cs`
Create `Assets/Project/Scripts/AI/AIHealthBase.cs`:
```csharp
using System;
using UnityEngine;

/// <summary>
/// Abstract base class for plant monster health, damage handling, and death events.
/// Implements <see cref="IDamageable"/>.
/// </summary>
public abstract class AIHealthBase : MonoBehaviour, IDamageable
{
    #region Properties
    /// <summary>Gets or sets whether this plant is currently immune to all incoming damage.</summary>
    public abstract bool IsInvincible { get; set; }

    /// <summary>Gets the current hit points.</summary>
    public abstract int CurrentHp { get; }

    /// <summary>Gets the maximum hit points.</summary>
    public abstract int MaxHp { get; }
    #endregion

    #region Events
    /// <summary>Event raised when health reaches 0.</summary>
    public abstract event Action OnDieEvent;

    /// <summary>Event raised when damage is taken but entity survives.</summary>
    public abstract event Action OnTakeDamageEvent;
    #endregion

    #region Public Methods
    /// <summary>Initializes health with max hit points.</summary>
    /// <param name="maxHp">Maximum and starting HP.</param>
    public abstract void Initialize(int maxHp);

    /// <summary>Applies incoming damage to the plant.</summary>
    /// <param name="amount">Amount of damage to inflict.</param>
    public abstract void TakeDamage(int amount);
    #endregion
}
```

## 4. Verification & Testing Checklist
- [ ] All 5 abstract base classes compile cleanly with zero errors in Unity 6.3.
- [ ] XML documentation exists on all public members.
- [ ] Strict casing and naming rules observed.
