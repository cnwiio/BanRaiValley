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
