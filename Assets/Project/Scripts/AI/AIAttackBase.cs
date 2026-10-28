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
