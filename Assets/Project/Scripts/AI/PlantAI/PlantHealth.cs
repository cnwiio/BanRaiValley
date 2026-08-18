using System;
using UnityEngine;

/// <summary>
/// Manages hit points, damage processing, and death detection for a Plant AI entity.
/// Implements <see cref="IDamageable"/> and dispatches both local C# events and global
/// <see cref="EventBus{T}"/> structs on damage and death.
/// No <c>Update</c> polling — all state changes are event-driven.
/// </summary>
public class PlantHealth : MonoBehaviour, IDamageable
{
    #region Private Fields

    private float _maxHp;
    private float _currentHp;
    private bool _isInvulnerable;

    #endregion

    #region Public Properties (IDamageable + Extensions)

    /// <summary>Current hit-point total. Read-only to external systems.</summary>
    public float CurrentHp => _currentHp;

    /// <summary>Maximum hit-point capacity.</summary>
    public float MaxHp => _maxHp;

    /// <summary>
    /// Returns <c>true</c> while the entity has more than zero HP and
    /// has not begun a death sequence.
    /// </summary>
    public bool IsAlive => _currentHp > 0f;

    /// <summary>
    /// Returns <c>true</c> when the plant is immune to all incoming damage
    /// (e.g. during awakening or stagger windows).
    /// </summary>
    public bool IsInvulnerable => _isInvulnerable;

    #endregion

    #region Events

    /// <summary>Raised locally each time this plant receives a valid damage hit.</summary>
    public event Action<DamageData> OnDamaged;

    /// <summary>Raised locally when this plant's HP reaches zero.</summary>
    public event Action OnDied;

    #endregion

    #region Public Methods

    /// <summary>
    /// Initialises the plant's health pool and resets invulnerability.
    /// Call once from the owning controller (e.g. PlantAIBrain) after instantiation.
    /// </summary>
    /// <param name="maxHp">Maximum hit points to assign.</param>
    public void Initialize(float maxHp)
    {
        _maxHp = maxHp;
        _currentHp = maxHp;
        _isInvulnerable = false;
    }

    /// <summary>
    /// Sets or clears invulnerability. Used to protect the plant during
    /// awakening animations or stagger recovery.
    /// </summary>
    /// <param name="isInvulnerable"><c>true</c> to block all incoming damage.</param>
    public void SetInvulnerable(bool isInvulnerable)
    {
        _isInvulnerable = isInvulnerable;
    }

    /// <summary>
    /// Applies the supplied damage to this plant entity.
    /// Guards against dead state, invulnerability, and non-positive amounts.
    /// Dispatches local events and global EventBus events.
    /// </summary>
    /// <param name="damageData">Full context of the incoming damage hit.</param>
    public void TakeDamage(DamageData damageData)
    {
        if (!IsAlive) return;
        if (_isInvulnerable) return;
        if (damageData.Amount <= 0f) return;

        ApplyDamage(damageData);
        DispatchDamagedEvents(damageData);

        if (_currentHp <= 0f)
        {
            DispatchDeathEvents();
        }
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Subtracts the damage amount from current HP, clamping to zero.
    /// </summary>
    private void ApplyDamage(DamageData damageData)
    {
        _currentHp = Mathf.Max(_currentHp - damageData.Amount, 0f);
    }

    /// <summary>
    /// Invokes the local OnDamaged event and raises the global OnPlantDamagedEvent.
    /// </summary>
    private void DispatchDamagedEvents(DamageData damageData)
    {
        OnDamaged?.Invoke(damageData);

        EventBus<OnPlantDamagedEvent>.Raise(new OnPlantDamagedEvent
        {
            PlantInstance = gameObject,
            DamageData = damageData,
            CurrentHp = _currentHp,
            MaxHp = _maxHp,
        });
    }

    /// <summary>
    /// Invokes the local OnDied event and raises the global OnPlantDiedEvent.
    /// </summary>
    private void DispatchDeathEvents()
    {
        OnDied?.Invoke();

        EventBus<OnPlantDiedEvent>.Raise(new OnPlantDiedEvent
        {
            PlantInstance = gameObject,
            Position = transform.position,
            CellPos = Vector3Int.zero, // CellPos to be set by the owning controller
        });
    }

    #endregion
}
