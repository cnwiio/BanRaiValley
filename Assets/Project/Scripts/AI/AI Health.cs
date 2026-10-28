using System;
using UnityEngine;

/// <summary>
/// Concrete health and damage handling component for plant monsters.
/// </summary>
public class AIHealth : AIHealthBase
{
    #region Fields
    private int _maxHp;
    private int _hp;
    #endregion

    #region Properties
    /// <summary>Gets or sets whether this plant is currently immune to all incoming damage.</summary>
    public override bool IsInvincible { get; set; }

    /// <summary>Backwards-compatible typo alias for IsInvincible.</summary>
    public bool IsInvicible
    {
        get => IsInvincible;
        set => IsInvincible = value;
    }

    /// <summary>Gets the current hit points.</summary>
    public override int CurrentHp => _hp;

    /// <summary>Gets the maximum hit points.</summary>
    public override int MaxHp => _maxHp;

    /// <summary>Backwards-compatible property for HP with clamp bounds.</summary>
    public int Hp
    {
        get => _hp;
        set => _hp = Mathf.Clamp(value, 0, _maxHp);
    }
    #endregion

    #region Events
    /// <summary>Event raised when health reaches 0.</summary>
    public override event Action OnDieEvent;

    /// <summary>Event raised when damage is taken but entity survives.</summary>
    public override event Action OnTakeDamageEvent;
    #endregion

    #region Public Methods
    /// <summary>
    /// Initializes health with max hit points.
    /// </summary>
    /// <param name="maxHp">Maximum and starting hit points.</param>
    public override void Initialize(int maxHp)
    {
        _maxHp = maxHp;
        _hp = maxHp;
    }

    /// <summary>
    /// Applies incoming damage to the plant.
    /// </summary>
    /// <param name="amount">Amount of damage to inflict.</param>
    public override void TakeDamage(int amount)
    {
        if (IsInvincible) return;

        Hp -= amount;

        if (Hp <= 0)
        {
            OnDieEvent?.Invoke();
            return;
        }

        OnTakeDamageEvent?.Invoke();
    }
    #endregion
}
