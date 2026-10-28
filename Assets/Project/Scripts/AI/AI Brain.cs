using System.Collections;
using UnityEngine;
using Lean.Pool;

/// <summary>
/// Concrete coordinator for plant monster AI behavior.
/// Implements coroutine loops for chasing, animation speed syncing, hit stun recovery, and death drops.
/// </summary>
public class AIBrain : AIBrainBase
{
    #region Constants
    private const float MOVE_THRESHOLD = 1.5f;
    private const float DE_SPAWN_DELAY_SEC = 3f;
    private const float CHASE_UPDATE_INTERVAL_SEC = 0.25f;
    #endregion

    #region Fields
    private readonly WaitForSeconds _chaseWaitInterval = new WaitForSeconds(CHASE_UPDATE_INTERVAL_SEC);
    private WaitForSeconds _stunWaitTime;
    private readonly WaitForSeconds _deSpawnWaitTime = new WaitForSeconds(DE_SPAWN_DELAY_SEC);

    private Coroutine _chaseCoroutine;
    private Coroutine _movementCoroutine;
    private Coroutine _stunCoroutine;
    private Coroutine _dieCoroutine;

    private Vector3 _lastKnownTargetPos;
    #endregion

    #region Unity Messages
    protected override void Awake()
    {
        base.Awake();

        if (data != null)
        {
            _stunWaitTime = new WaitForSeconds(data.StunTime);
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        StopAllCoroutinesSafely();
    }
    #endregion

    #region State Machine Hooks
    /// <summary>
    /// Handles entering a state by starting appropriate actions and coroutines.
    /// </summary>
    protected override void OnEnterState(PlantAIState state)
    {
        switch (state)
        {
            case PlantAIState.Awake:
                if (health != null) health.IsInvincible = true;
                if (detection != null) detection.EnableDetect(false);
                if (animator != null) animator.PlayAwaken();
                break;

            case PlantAIState.Idle:
                break;

            case PlantAIState.Chase:
                _movementCoroutine = StartCoroutine(AnimationSpeedCoroutine());
                break;

            case PlantAIState.Attack:
                if (movement != null) movement.RotateToTarget();
                if (attack != null) attack.StartAttack();
                if (animator != null) animator.PlayAttack();
                break;

            case PlantAIState.Stun:
                if (movement != null)
                {
                    movement.StopMoving();
                    movement.RotateToTarget();
                }
                if (attack != null) attack.StopAttack();
                if (animator != null) animator.PlayHit();
                _stunCoroutine = StartCoroutine(StunCoroutine());
                break;

            case PlantAIState.Die:
                if (movement != null) movement.StopMoving();
                if (attack != null) attack.StopAttack();
                if (animator != null) animator.PlayDie();
                _dieCoroutine = StartCoroutine(DieCoroutine());
                break;
        }
    }

    /// <summary>
    /// Handles exiting a state by stopping active coroutines and resetting states.
    /// </summary>
    protected override void OnExitState(PlantAIState state)
    {
        switch (state)
        {
            case PlantAIState.Awake:
                if (health != null) health.IsInvincible = false;
                break;

            case PlantAIState.Chase:
                if (_chaseCoroutine != null)
                {
                    StopCoroutine(_chaseCoroutine);
                    _chaseCoroutine = null;
                }
                if (_movementCoroutine != null)
                {
                    StopCoroutine(_movementCoroutine);
                    _movementCoroutine = null;
                }
                if (movement != null) movement.StopMoving();
                if (animator != null) animator.SetMoving(0f);
                break;

            case PlantAIState.Attack:
                break;

            case PlantAIState.Stun:
                if (_stunCoroutine != null)
                {
                    StopCoroutine(_stunCoroutine);
                    _stunCoroutine = null;
                }
                break;

            case PlantAIState.Die:
                if (_dieCoroutine != null)
                {
                    StopCoroutine(_dieCoroutine);
                    _dieCoroutine = null;
                }
                break;
        }
    }

    /// <summary>
    /// Handles entering a chase sub-state.
    /// </summary>
    protected override void EnterChaseState(PlantChaseState state)
    {
        switch (state)
        {
            case PlantChaseState.ChasePlayer:
                if (_chaseCoroutine != null)
                {
                    StopCoroutine(_chaseCoroutine);
                }
                _chaseCoroutine = StartCoroutine(ChaseTargetCoroutine());
                break;

            case PlantChaseState.ChaseHome:
                if (movement != null) movement.ReturnToStart();
                break;
        }
    }

    /// <summary>
    /// Handles exiting a chase sub-state.
    /// </summary>
    protected override void ExitChaseState(PlantChaseState state)
    {
        switch (state)
        {
            case PlantChaseState.ChasePlayer:
                if (_chaseCoroutine != null)
                {
                    StopCoroutine(_chaseCoroutine);
                    _chaseCoroutine = null;
                }
                break;

            case PlantChaseState.ChaseHome:
                break;
        }
    }
    #endregion

    #region Event Handlers
    /// <summary>
    /// Refreshes stun state if hit during active stun; otherwise transitions into stun.
    /// </summary>
    protected override void OnTakeDamage()
    {
        if (CurrentState == PlantAIState.Die) return;

        if (CurrentState == PlantAIState.Stun)
        {
            if (_stunCoroutine != null)
            {
                StopCoroutine(_stunCoroutine);
                _stunCoroutine = null;
            }

            if (movement != null) movement.RotateToTarget();
            if (animator != null) animator.PlayHit();
            _stunCoroutine = StartCoroutine(StunCoroutine());
            return;
        }

        ChangeState(PlantAIState.Stun);
    }
    #endregion

    #region Drop & Pool Lifecycle
    /// <summary>
    /// Initializes all modular components with data asset configuration.
    /// </summary>
    protected override void Initialize()
    {
        if (data == null)
        {
            Debug.LogError($"[AIBrain] Missing AIData on {gameObject.name}");
            return;
        }

        if (_stunWaitTime == null)
        {
            _stunWaitTime = new WaitForSeconds(data.StunTime);
        }

        if (detection != null) detection.Initialize(data.DetectionRange);
        if (movement != null) movement.Initialize(transform.position, data);
        if (attack != null) attack.Initialize(data);
        if (health != null) health.Initialize(data.HP);

        _lastKnownTargetPos = Vector3.zero;
    }
    #endregion

    #region Coroutines
    private IEnumerator ChaseTargetCoroutine()
    {
        if (movement == null || movement.TargetTransform == null)
        {
            Debug.LogWarning($"[AIBrain] No target transform bound on {gameObject.name}");
            yield break;
        }

        Transform target = movement.TargetTransform;
        while (CurrentState == PlantAIState.Chase)
        {
            if (target == null) yield break;

            float distanceFromPlayer = Vector3.Distance(transform.position, target.position);
            if (distanceFromPlayer <= data.AttackRange)
            {
                if (attack != null && (attack.CanAttack || CurrentState == PlantAIState.Chase))
                {
                    ChangeState(PlantAIState.Attack);
                }
            }
            else
            {
                float distanceFromLastPos = Vector3.Distance(target.position, _lastKnownTargetPos);
                if (distanceFromLastPos > MOVE_THRESHOLD || _lastKnownTargetPos == Vector3.zero)
                {
                    _lastKnownTargetPos = target.position;
                    movement.MoveTo(target.position);
                }
            }

            yield return _chaseWaitInterval;
        }
    }

    private IEnumerator AnimationSpeedCoroutine()
    {
        while (true)
        {
            if (CurrentChaseState == PlantChaseState.ChaseHome && movement != null && movement.IsReachDestination)
            {
                ChangeState(PlantAIState.Idle);
            }

            if (animator != null && movement != null)
            {
                animator.SetMoving(movement.Speed);
            }

            yield return _chaseWaitInterval;
        }
    }

    private IEnumerator StunCoroutine()
    {
        yield return _stunWaitTime;
        _stunCoroutine = null;

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

    private IEnumerator DieCoroutine()
    {
        yield return _deSpawnWaitTime;
        RollDrop();
        LeanPool.Despawn(this);
        _dieCoroutine = null;
    }

    private void StopAllCoroutinesSafely()
    {
        if (_chaseCoroutine != null)
        {
            StopCoroutine(_chaseCoroutine);
            _chaseCoroutine = null;
        }

        if (_movementCoroutine != null)
        {
            StopCoroutine(_movementCoroutine);
            _movementCoroutine = null;
        }

        if (_stunCoroutine != null)
        {
            StopCoroutine(_stunCoroutine);
            _stunCoroutine = null;
        }

        if (_dieCoroutine != null)
        {
            StopCoroutine(_dieCoroutine);
            _dieCoroutine = null;
        }
    }
    #endregion
}
