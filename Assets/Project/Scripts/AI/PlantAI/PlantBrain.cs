using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Central decision coordinator for a plant AI entity. Implements <see cref="IAIAgent"/>
/// and wires together <see cref="PlantHealth"/>, <see cref="PlantPerception"/>,
/// <see cref="PlantMovement"/>, <see cref="PlantCombat"/>, and
/// <see cref="PlantAnimationController"/> into an event-driven Finite State Machine.
/// No <c>Update</c> polling is used for AI decisions; all transitions are triggered by
/// component events or timed coroutines.
/// </summary>
public class PlantBrain : MonoBehaviour, IAIAgent
{
    #region Serialized Fields

    [Header("Configuration")]
    [Tooltip("ScriptableObject containing all balance metrics for this plant variant.")]
    [SerializeField] private PlantAIConfigSO _config;

    [Tooltip("ScriptableObject defining the loot table evaluated on death.")]
    [SerializeField] private PlantLootTableSO _lootTable;

    [Header("Internal Subsystems")]
    [Tooltip("Health component managing HP, damage processing, and death detection.")]
    [SerializeField] private PlantHealth _health;

    [Tooltip("Perception component managing aggro trigger and target tracking.")]
    [SerializeField] private PlantPerception _perception;

    [Tooltip("Movement component encapsulating NavMeshAgent locomotion.")]
    [SerializeField] private PlantMovement _movement;

    [Tooltip("Combat component managing melee attack sequences and cooldowns.")]
    [SerializeField] private PlantCombat _combat;

    [Tooltip("Animation controller wrapping pre-hashed Animator parameter access.")]
    [SerializeField] private PlantAnimationController _animationController;

    [Tooltip("Collider(s) to disable when the plant dies. Prevents further interactions.")]
    [SerializeField] private Collider[] _bodyColliders;

    [Tooltip("Duration in seconds the plant's corpse remains before being despawned.")]
    [SerializeField] private float _despawnDelaySec = 4f;

    #endregion

    #region Properties

    /// <summary>The agent's current high-level behavioural state.</summary>
    public PlantAIState CurrentState { get; private set; } = PlantAIState.Dormant;

    /// <summary>The Transform of the entity this agent is currently targeting, or <c>null</c> if none.</summary>
    public Transform TargetTransform => _perception.CurrentTarget;

    #endregion

    #region Fields

    private Coroutine _awakeningCoroutine;
    private Coroutine _chaseCoroutine;
    private Coroutine _hitReactCoroutine;
    private Coroutine _despawnCoroutine;

    private const float CHASE_UPDATE_INTERVAL_SEC = 0.25f;

    #endregion

    #region Unity Messages

    private void Awake()
    {
        Debug.Log("Awake");
        _health.Initialize(_config.MaxHp);
        _perception.Initialize(_config.AggroRadiusM);
        _movement.Initialize(_config.MoveSpeedUps, _config.StoppingDistanceM, _config.RotationSpeedDeg);
        _combat.Initialize(_config, gameObject);

        // Perception is disabled during dormancy; it is enabled once Awakening completes.
        _perception.SetPerceptionActive(false);
    }

    private void OnEnable()
    {
        _health.OnDamaged     += OnDamaged;
        _health.OnDied        += OnDied;
        _perception.OnTargetDetected += OnTargetDetected;
        _perception.OnTargetLost     += OnTargetLost;
        _combat.OnAttackCompleted    += OnAttackCompleted;
    }

    private void OnDisable()
    {
        _health.OnDamaged     -= OnDamaged;
        _health.OnDied        -= OnDied;
        _perception.OnTargetDetected -= OnTargetDetected;
        _perception.OnTargetLost     -= OnTargetLost;
        _combat.OnAttackCompleted    -= OnAttackCompleted;

        StopAllStateCoroutines();
    }

    #endregion

    #region Public Methods — IAIAgent

    /// <summary>
    /// Assigns a new target to this agent. Triggers an immediate transition to <see cref="PlantAIState.Chase"/>
    /// if the plant is currently in <c>Idle</c>.
    /// </summary>
    /// <param name="target">Transform of the target entity to pursue.</param>
    public void SetTarget(Transform target)
    {
        if (target == null)
        {
            return;
        }

        if (CurrentState == PlantAIState.Idle)
        {
            OnTargetDetected(target);
        }
    }

    /// <summary>
    /// Removes the current target and returns the agent to <see cref="PlantAIState.Idle"/>
    /// if it is currently chasing.
    /// </summary>
    public void ClearTarget()
    {
        _perception.ClearTarget();
    }

    #endregion

    #region Public Methods — Awakening

    /// <summary>
    /// Initiates the awakening sequence: sets invulnerability, plays the uprooting animation,
    /// waits for the configured duration, then enables perception and transitions to <c>Idle</c>.
    /// Call this once from whatever external trigger reveals the plant to the player.
    /// </summary>
    public void Awaken()
    {
        TransitionTo(PlantAIState.Awakening);
    }

    #endregion

    #region Private Methods — State Transition Engine

    /// <summary>
    /// Executes exit logic for the current state, enters the new state with its setup logic,
    /// emits <see cref="OnPlantStateChangedEvent"/> on the global EventBus, then updates
    /// <see cref="CurrentState"/>. No-ops if the target state is identical to the current state.
    /// </summary>
    /// <param name="nextState">The state to transition into.</param>
    private void TransitionTo(PlantAIState nextState)
    {
        if (CurrentState == nextState)
        {
            return;
        }

        PlantAIState previousState = CurrentState;

        // --- Exit logic for current state ---
        ExitState(previousState);

        // --- Emit global event before updating CurrentState so listeners see both values ---
        EventBus<OnPlantStateChangedEvent>.Raise(new OnPlantStateChangedEvent
        {
            PlantInstance = gameObject,
            PreviousState = previousState,
            NewState      = nextState,
        });

        CurrentState = nextState;

        // --- Enter logic for new state ---
        EnterState(nextState);
    }

    private void ExitState(PlantAIState state)
    {
        switch (state)
        {
            case PlantAIState.Chase:
                StopTrackedCoroutine(ref _chaseCoroutine);
                break;

            case PlantAIState.Awakening:
                StopTrackedCoroutine(ref _awakeningCoroutine);
                break;

            case PlantAIState.HitReact:
                StopTrackedCoroutine(ref _hitReactCoroutine);
                break;
        }
    }

    private void EnterState(PlantAIState state)
    {
        switch (state)
        {
            case PlantAIState.Awakening:
                EnterAwakening();
                break;

            case PlantAIState.Idle:
                EnterIdle();
                break;

            case PlantAIState.Chase:
                EnterChase();
                break;

            case PlantAIState.Attack:
                EnterAttack();
                break;

            case PlantAIState.HitReact:
                EnterHitReact();
                break;

            case PlantAIState.Dead:
                EnterDead();
                break;
        }
    }

    #endregion

    #region Private Methods — State Enter Implementations

    private void EnterAwakening()
    {
        _health.SetInvulnerable(true);
        _animationController.PlayAwaken();
        _awakeningCoroutine = StartCoroutine(AwakeningRoutine());
    }

    private void EnterIdle()
    {
        _movement.StopMovement();
        _animationController.SetMoving(false);
    }

    private void EnterChase()
    {
        _animationController.SetMoving(true);
        Transform target = _perception.CurrentTarget;

        if (target != null)
        {
            _chaseCoroutine = StartCoroutine(ChaseTargetRoutine(target));
        }
    }

    private void EnterAttack()
    {
        _animationController.SetMoving(false);
        _animationController.PlayAttack();

        Transform target = _perception.CurrentTarget;
        if (target != null)
        {
            _combat.ExecuteMeleeAttack(target);
        }
    }

    private void EnterHitReact()
    {
        _animationController.PlayHit();
        _hitReactCoroutine = StartCoroutine(HitReactRoutine());
    }

    private void EnterDead()
    {
        _movement.StopMovement();
        _combat.StopAllCombatRoutines();
        _perception.SetPerceptionActive(false);
        _animationController.SetMoving(false);
        _animationController.PlayDie();

        foreach (Collider col in _bodyColliders)
        {
            if (col != null)
            {
                col.enabled = false;
            }
        }

        // Evaluate and log drops (caller responsible for spawning items).
        List<Item> drops = _lootTable.EvaluateDrops();
        Debug.Log($"[PlantBrain] Death loot drops evaluated: {drops.Count} item(s) from {gameObject.name}.");

        _despawnCoroutine = StartCoroutine(DespawnRoutine());
    }

    #endregion

    #region Coroutines

    /// <summary>
    /// Waits for the configured awakening animation duration, removes invulnerability,
    /// enables perception, then transitions the plant to <see cref="PlantAIState.Idle"/>.
    /// </summary>
    private IEnumerator AwakeningRoutine()
    {
        yield return new WaitForSeconds(_config.AwakeningDurationSec);

        _health.SetInvulnerable(false);
        _perception.SetPerceptionActive(true);
        _awakeningCoroutine = null;

        TransitionTo(PlantAIState.Idle);
    }

    /// <summary>
    /// Periodically updates the plant's navigation destination toward the target every
    /// <see cref="CHASE_UPDATE_INTERVAL_SEC"/> seconds. When the target enters attack range
    /// and the combat cooldown has elapsed, stops movement and triggers an attack.
    /// </summary>
    /// <param name="target">The Transform to pursue.</param>
    private IEnumerator ChaseTargetRoutine(Transform target)
    {
        // Use a cached WaitForSeconds to avoid per-iteration GC allocations.
        var waitInterval = new WaitForSeconds(CHASE_UPDATE_INTERVAL_SEC);

        while (target != null && target.gameObject.activeInHierarchy)
        {
            float distanceSqr     = (target.position - transform.position).sqrMagnitude;
            float attackRangeSqr  = _config.AttackRangeM * _config.AttackRangeM;

            if (distanceSqr <= attackRangeSqr && _combat.CanAttack)
            {
                _movement.StopMovement();
                TransitionTo(PlantAIState.Attack);
                yield break;
            }
            
            // Debug.Log(target.position);
            _movement.SetDestination(target.position);
            yield return waitInterval;
        }

        // Target became null or inactive — fall back to Idle.
        _chaseCoroutine = null;
        TransitionTo(PlantAIState.Idle);
    }

    /// <summary>
    /// Waits for the stagger duration defined in config, then resumes chasing if a target
    /// is still in perception range, or returns to <see cref="PlantAIState.Idle"/> if not.
    /// </summary>
    private IEnumerator HitReactRoutine()
    {
        yield return new WaitForSeconds(_config.HitStaggerDurationSec);
        _hitReactCoroutine = null;

        if (_perception.HasTarget)
        {
            TransitionTo(PlantAIState.Chase);
        }
        else
        {
            TransitionTo(PlantAIState.Idle);
        }
    }

    /// <summary>
    /// Waits for the despawn delay then deactivates the plant's GameObject.
    /// </summary>
    private IEnumerator DespawnRoutine()
    {
        yield return new WaitForSeconds(_despawnDelaySec);
        _despawnCoroutine = null;
        gameObject.SetActive(false);
    }

    #endregion

    #region Event Handlers

    private void OnTargetDetected(Transform target)
    {
        Debug.Log("Detect");
        if (CurrentState == PlantAIState.Idle || CurrentState == PlantAIState.Dormant)
        {
            TransitionTo(PlantAIState.Chase);
        }
    }

    private void OnTargetLost()
    {
        if (CurrentState == PlantAIState.Chase)
        {
            StopTrackedCoroutine(ref _chaseCoroutine);
            _movement.StopMovement();
            TransitionTo(PlantAIState.Idle);
        }
    }

    private void OnAttackCompleted()
    {
        if (CurrentState != PlantAIState.Attack)
        {
            return;
        }

        if (_perception.HasTarget)
        {
            TransitionTo(PlantAIState.Chase);
        }
        else
        {
            TransitionTo(PlantAIState.Idle);
        }
    }

    private void OnDamaged(DamageData damageData)
    {
        // Do not interrupt an attack or a death sequence with a hit-react.
        if (CurrentState == PlantAIState.Dead || CurrentState == PlantAIState.Awakening)
        {
            return;
        }

        if (CurrentState != PlantAIState.Attack)
        {
            TransitionTo(PlantAIState.HitReact);
        }
        else
        {
            // Play the hit animation visually but don't break the attack flow.
            _animationController.PlayHit();
        }
    }

    private void OnDied()
    {
        TransitionTo(PlantAIState.Dead);
    }

    #endregion

    #region Private Methods — Utilities

    /// <summary>
    /// Stops and nullifies a coroutine reference in a single safe call.
    /// </summary>
    /// <param name="coroutine">Reference to the coroutine field to stop and clear.</param>
    private void StopTrackedCoroutine(ref Coroutine coroutine)
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }
    }

    /// <summary>
    /// Stops all active state coroutines. Called from <c>OnDisable</c> to prevent memory leaks.
    /// </summary>
    private void StopAllStateCoroutines()
    {
        StopTrackedCoroutine(ref _awakeningCoroutine);
        StopTrackedCoroutine(ref _chaseCoroutine);
        StopTrackedCoroutine(ref _hitReactCoroutine);
        StopTrackedCoroutine(ref _despawnCoroutine);
        _combat.StopAllCombatRoutines();
    }

    #endregion
}
