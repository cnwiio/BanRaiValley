using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Concrete melee attack component for plant monsters driving hitbox timing and attack cooldowns.
/// </summary>
public class AIAttack : AIAttackBase
{
    #region Constants
    private const float HITBOX_LIFESPAN = 0.2f;
    #endregion

    #region Serialized Fields
    [Header("Combat")]
    [Tooltip("Hitbox trigger volume enabled during attack delivery.")]
    [SerializeField] private AttackHitBox hitBox;
    #endregion

    #region Fields
    private bool _canAttack = true;
    private bool _isAttacking;
    private float _attackCooldown;
    private float _hitboxLifeSpan = HITBOX_LIFESPAN;

    private Coroutine _attackCoroutine;
    private Coroutine _cooldownCoroutine;
    #endregion

    #region Properties
    /// <summary>Gets whether the plant is ready to initiate an attack (cooldown elapsed).</summary>
    public override bool CanAttack => _canAttack;

    /// <summary>Gets whether an attack sequence is actively in progress.</summary>
    public override bool IsAttacking => _isAttacking;
    #endregion

    #region Events
    /// <summary>Event raised when the attack sequence completes its recovery/cooldown.</summary>
    public override event Action OnAttackEndEvent;
    #endregion

    #region Unity Messages
    private void OnDisable()
    {
        StopAttack();
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Initializes attack parameters from configuration data.
    /// </summary>
    /// <param name="data">Data asset containing attack cooldown and damage stats.</param>
    public override void Initialize(AIData data)
    {
        if (data != null)
        {
            _attackCooldown = data.AttackCooldown;
            _hitboxLifeSpan = HITBOX_LIFESPAN;
            if (hitBox != null)
            {
                hitBox.Initialize(data.Damage);
            }
        }
    }

    /// <summary>
    /// Backwards-compatible initialization overload accepting discrete combat parameters.
    /// </summary>
    public void Initialize(float cooldown, float hitboxLife, int damage)
    {
        _attackCooldown = cooldown;
        _hitboxLifeSpan = hitboxLife;
        if (hitBox != null)
        {
            hitBox.Initialize(damage);
        }
    }

    /// <summary>Prepares the plant to attack, consuming availability until cooldown completes.</summary>
    public override void StartAttack()
    {
        if (!_canAttack || _isAttacking) return;

        _canAttack = false;
        _isAttacking = true;
    }

    /// <summary>Executes the actual damage delivery via the hitbox coroutine.</summary>
    public override void ExecuteAttack()
    {
        if (_attackCoroutine != null)
        {
            StopCoroutine(_attackCoroutine);
        }

        _attackCoroutine = StartCoroutine(AttackCoroutine());
    }

    /// <summary>Backwards-compatible typo alias for ExecuteAttack.</summary>
    [Obsolete("Use ExecuteAttack instead")]
    public void ExcuteAttack()
    {
        ExecuteAttack();
    }

    /// <summary>Interrupts and cancels any active attack or cooldown sequence immediately.</summary>
    public override void StopAttack()
    {
        if (_attackCoroutine != null)
        {
            StopCoroutine(_attackCoroutine);
            _attackCoroutine = null;
        }

        if (_cooldownCoroutine != null)
        {
            StopCoroutine(_cooldownCoroutine);
            _cooldownCoroutine = null;
        }

        if (hitBox != null)
        {
            hitBox.EnableHitBox(false);
        }

        _isAttacking = false;
        _canAttack = true;
    }
    #endregion

    #region Coroutines
    private IEnumerator AttackCoroutine()
    {
        if (hitBox != null)
        {
            hitBox.EnableHitBox(true);
        }

        yield return new WaitForSeconds(_hitboxLifeSpan);

        if (hitBox != null)
        {
            hitBox.EnableHitBox(false);
        }

        _attackCoroutine = null;
        OnAttackEndEvent?.Invoke();

        if (_cooldownCoroutine != null)
        {
            StopCoroutine(_cooldownCoroutine);
        }

        _cooldownCoroutine = StartCoroutine(AttackCooldownRoutine());
    }

    private IEnumerator AttackCooldownRoutine()
    {
        yield return new WaitForSeconds(_attackCooldown);
        _canAttack = true;
        _isAttacking = false;
        _cooldownCoroutine = null;
    }
    #endregion
}
