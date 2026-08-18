using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Action-layer component that orchestrates melee attack sequences for the plant AI.
/// Manages the windup → hitbox-active → recovery → cooldown pipeline entirely via
/// coroutines. No polling occurs in <c>Update</c>.
/// </summary>
public class PlantCombat : MonoBehaviour
{
    #region Serialized Fields

    [Header("Combat References")]
    [Tooltip("The melee hitbox component on this plant's attack collider child object.")]
    [SerializeField] private PlantMeleeHitbox _meleeHitbox;

    [Tooltip("Transform used as the origin point when a projectile variant is added in future tasks.")]
    [SerializeField] private Transform _projectileSpawnPoint;

    #endregion

    #region Properties

    /// <summary>Returns <c>true</c> when the plant is currently executing an attack sequence.</summary>
    public bool IsAttacking => _isAttacking;

    /// <summary>Returns <c>true</c> when the cooldown has elapsed and a new attack may be started.</summary>
    public bool CanAttack => _canAttack;

    #endregion

    #region Events

    /// <summary>Raised at the moment an attack sequence begins (windup starts).</summary>
    public event Action OnAttackStarted;

    /// <summary>Raised when the attack sequence fully completes and the cooldown begins.</summary>
    public event Action OnAttackCompleted;

    #endregion

    #region Fields

    private PlantAIConfigSO _config;
    private bool            _isAttacking;
    private bool            _canAttack = true;

    private Coroutine _meleeAttackCoroutine;
    private Coroutine _cooldownCoroutine;

    #endregion

    #region Unity Messages

    private void OnDisable()
    {
        StopAllCombatRoutines();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Binds the combat component to the owning plant's configuration and initializes the melee hitbox.
    /// Must be called by the owning controller before any attack can be requested.
    /// </summary>
    /// <param name="config">ScriptableObject containing attack damage, windup, and cooldown values.</param>
    /// <param name="owner">The plant's root GameObject, forwarded to <see cref="PlantMeleeHitbox"/> to prevent self-hits.</param>
    public void Initialize(PlantAIConfigSO config, GameObject owner)
    {
        _config = config;

        if (_meleeHitbox != null)
        {
            _meleeHitbox.Initialize(owner, _config.BaseAttackDamage);
            _meleeHitbox.EnableHitbox(false);
        }
    }

    /// <summary>
    /// Requests the start of a melee attack sequence aimed at the specified target.
    /// Does nothing if an attack is already in progress or the cooldown has not expired.
    /// </summary>
    /// <param name="target">Transform of the entity to attack. Used for future directional validation.</param>
    public void ExecuteMeleeAttack(Transform target)
    {
        if (!_canAttack || _isAttacking)
        {
            return;
        }

        if (_meleeAttackCoroutine != null)
        {
            StopCoroutine(_meleeAttackCoroutine);
        }

        _meleeAttackCoroutine = StartCoroutine(MeleeAttackRoutine(target));
    }

    /// <summary>
    /// Immediately aborts any in-progress attack or cooldown coroutines and ensures
    /// the hitbox is disabled. Safe to call from <c>OnDisable</c> or state transitions.
    /// </summary>
    public void StopAllCombatRoutines()
    {
        if (_meleeAttackCoroutine != null)
        {
            StopCoroutine(_meleeAttackCoroutine);
            _meleeAttackCoroutine = null;
        }

        if (_cooldownCoroutine != null)
        {
            StopCoroutine(_cooldownCoroutine);
            _cooldownCoroutine = null;
        }

        if (_meleeHitbox != null)
        {
            _meleeHitbox.EnableHitbox(false);
        }

        _isAttacking = false;
    }

    #endregion

    #region Coroutines

    /// <summary>
    /// Full melee attack pipeline: windup delay → hitbox active → brief active window → recovery → cooldown.
    /// </summary>
    /// <param name="target">Attack target, reserved for directional or area-limit logic in future iterations.</param>
    private IEnumerator MeleeAttackRoutine(Transform target)
    {
        _isAttacking = true;
        _canAttack   = false;

        OnAttackStarted?.Invoke();

        // Windup: wait for the animation to reach the impact frame.
        yield return new WaitForSeconds(_config.AttackWindupSec);

        // Activate hitbox for a brief window to register hits.
        _meleeHitbox.EnableHitbox(true);
        yield return new WaitForSeconds(0.2f);
        _meleeHitbox.EnableHitbox(false);

        _isAttacking = false;

        OnAttackCompleted?.Invoke();

        // Begin cooldown on a separate tracked coroutine so it survives state transitions
        // that may call StopAllCombatRoutines (which will also cancel the cooldown if needed).
        _meleeAttackCoroutine = null;

        if (_cooldownCoroutine != null)
        {
            StopCoroutine(_cooldownCoroutine);
        }

        _cooldownCoroutine = StartCoroutine(AttackCooldownRoutine());
    }

    /// <summary>
    /// Waits for the configured cooldown duration, then re-enables attack availability.
    /// </summary>
    private IEnumerator AttackCooldownRoutine()
    {
        yield return new WaitForSeconds(_config.AttackCooldownSec);
        _canAttack        = true;
        _cooldownCoroutine = null;
    }

    #endregion
}
