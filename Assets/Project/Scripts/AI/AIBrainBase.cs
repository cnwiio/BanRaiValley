using System;
using UnityEngine;
using Lean.Pool;

public enum PlantAIState
{
    Awake,
    Idle,
    Chase,
    Attack,
    Stun,
    Die
}

public enum PlantChaseState
{
    ChasePlayer,
    ChaseHome
}

/// <summary>
/// Abstract coordinator base class for plant monster AI.
/// Manages the plant state machine, lifecycle, pool recycling, and component event wiring.
/// </summary>
public abstract class AIBrainBase : MonoBehaviour, IPoolable
{
    #region Serialized Fields
    [Header("Data")]
    [Tooltip("Configuration data asset specifying stats, drops, and intervals.")]
    [SerializeField] protected AIData data;

    [Header("Modular Components")]
    [Tooltip("Locomotion and navigation component.")]
    [SerializeField] protected AIMovementBase movement;

    [Tooltip("Perception and player detection component.")]
    [SerializeField] protected AIDetectionBase detection;

    [Tooltip("Combat and damage delivery component.")]
    [SerializeField] protected AIAttackBase attack;

    [Tooltip("Skeletal animation coordinator component.")]
    [SerializeField] protected AIAnimationControllerBase animator;

    [Tooltip("Health, damage reception, and death handler.")]
    [SerializeField] protected AIHealthBase health;
    #endregion

    #region Properties
    /// <summary>Gets the current state of the plant AI state machine.</summary>
    public PlantAIState CurrentState { get; protected set; } = PlantAIState.Idle;

    /// <summary>Gets the current sub-state when navigating or chasing.</summary>
    public PlantChaseState CurrentChaseState { get; protected set; } = PlantChaseState.ChasePlayer;

    /// <summary>Protected alias property routing state assignment through ChangeState.</summary>
    protected PlantAIState currentState
    {
        get => CurrentState;
        set => ChangeState(value);
    }

    /// <summary>Protected alias property routing chase state assignment through ChangeChaseState.</summary>
    protected PlantChaseState currentChaseState
    {
        get => CurrentChaseState;
        set => ChangeChaseState(value);
    }
    #endregion

    #region Unity Messages
    protected virtual void Awake()
    {
        if (movement == null) movement = GetComponent<AIMovementBase>();
        if (detection == null) detection = GetComponent<AIDetectionBase>();
        if (attack == null) attack = GetComponent<AIAttackBase>();
        if (animator == null) animator = GetComponent<AIAnimationControllerBase>();
        if (health == null) health = GetComponent<AIHealthBase>();
    }

    protected virtual void OnEnable()
    {
        if (detection != null)
        {
            detection.OnTargetDetectedEvent += OnTargetDetected;
            detection.OnTargetLostEvent += OnTargetLost;
        }

        if (attack != null)
        {
            attack.OnAttackEndEvent += OnAttackEnd;
        }

        if (health != null)
        {
            health.OnTakeDamageEvent += OnTakeDamage;
            health.OnDieEvent += OnDie;
        }
    }

    protected virtual void OnDisable()
    {
        if (detection != null)
        {
            detection.OnTargetDetectedEvent -= OnTargetDetected;
            detection.OnTargetLostEvent -= OnTargetLost;
        }

        if (attack != null)
        {
            attack.OnAttackEndEvent -= OnAttackEnd;
        }

        if (health != null)
        {
            health.OnTakeDamageEvent -= OnTakeDamage;
            health.OnDieEvent -= OnDie;
        }
    }
    #endregion

    #region State Machine
    /// <summary>
    /// Transitions the AI state machine to the specified next state.
    /// </summary>
    /// <param name="nextState">Target state to transition to.</param>
    public virtual void ChangeState(PlantAIState nextState)
    {
        if (CurrentState == nextState) return;

        OnExitState(CurrentState);
        CurrentState = nextState;
        OnEnterState(nextState);
    }

    /// <summary>
    /// Transitions the chasing sub-state machine to the specified sub-state.
    /// </summary>
    /// <param name="nextChaseState">Target chase sub-state to transition to.</param>
    public virtual void ChangeChaseState(PlantChaseState nextChaseState)
    {
        ExitChaseState(CurrentChaseState);
        CurrentChaseState = nextChaseState;
        EnterChaseState(nextChaseState);
    }

    /// <summary>Invoked immediately upon entering a new state.</summary>
    protected virtual void OnEnterState(PlantAIState state) { }

    /// <summary>Invoked immediately before exiting an active state.</summary>
    protected virtual void OnExitState(PlantAIState state) { }

    /// <summary>Invoked immediately upon entering a new chase sub-state.</summary>
    protected virtual void EnterChaseState(PlantChaseState state) { }

    /// <summary>Invoked immediately before exiting an active chase sub-state.</summary>
    protected virtual void ExitChaseState(PlantChaseState state) { }
    #endregion

    #region Event Handlers
    /// <summary>Callback when the perception component spots a target.</summary>
    protected virtual void OnTargetDetected(Transform target)
    {
        if (CurrentState == PlantAIState.Idle ||
            CurrentState == PlantAIState.Awake ||
            (CurrentState == PlantAIState.Chase && CurrentChaseState == PlantChaseState.ChaseHome))
        {
            ChangeState(PlantAIState.Chase);
            if (movement != null)
            {
                movement.BindTargetTransform(target);
            }
            ChangeChaseState(PlantChaseState.ChasePlayer);
        }
    }

    /// <summary>Callback when the perception component loses target line-of-sight.</summary>
    protected virtual void OnTargetLost()
    {
        if (CurrentState != PlantAIState.Chase) return;
        ChangeChaseState(PlantChaseState.ChaseHome);
    }

    /// <summary>Callback when the attack sequence completes its recovery/cooldown.</summary>
    protected virtual void OnAttackEnd()
    {
        if (CurrentState != PlantAIState.Attack) return;
        ChangeState(PlantAIState.Chase);
        if (detection != null && detection.IsPlayerInSight)
        {
            ChangeChaseState(PlantChaseState.ChasePlayer);
        }
        else
        {
            ChangeChaseState(PlantChaseState.ChaseHome);
        }
    }

    /// <summary>Animation hit event trigger hook forwarding to attack delivery.</summary>
    public virtual void OnAttackAnimationHit()
    {
        if (CurrentState != PlantAIState.Attack) return;
        if (attack != null)
        {
            attack.ExecuteAttack();
        }
    }

    /// <summary>Animation event trigger hook when the awaken sequence finishes.</summary>
    public virtual void OnAwakeAnimationEnd()
    {
        ChangeState(PlantAIState.Idle);
        if (detection != null)
        {
            detection.EnableDetect(true);
        }
    }

    /// <summary>Callback when incoming damage is sustained.</summary>
    protected virtual void OnTakeDamage()
    {
        if (CurrentState == PlantAIState.Die) return;
        ChangeState(PlantAIState.Stun);
    }

    /// <summary>Callback when health reaches 0.</summary>
    protected virtual void OnDie()
    {
        ChangeState(PlantAIState.Die);
    }
    #endregion

    #region Drop & Pool Lifecycle
    /// <summary>Initializes component data and configuration.</summary>
    protected abstract void Initialize();

    /// <summary>Rolls drop chance and raises item pickup event if successful.</summary>
    protected virtual void RollDrop()
    {
        if (data == null || data.dropItem == null) return;
        if (UnityEngine.Random.value > data.dropChance) return;

        int amount = UnityEngine.Random.Range(data.dropAmountMin, data.dropAmountMax + 1);
        if (amount <= 0) return;

        EventBus<OnItemPickupEvent>.Raise(new OnItemPickupEvent
        {
            item = data.dropItem,
            amount = amount
        });
    }

    /// <summary>Called by LeanPool when spawned.</summary>
    public virtual void OnSpawn()
    {
        Initialize();
        ChangeState(PlantAIState.Awake);
    }

    /// <summary>Called by LeanPool when despawned.</summary>
    public virtual void OnDespawn()
    {
        if (animator != null)
        {
            animator.Reset();
        }
    }
    #endregion
}
