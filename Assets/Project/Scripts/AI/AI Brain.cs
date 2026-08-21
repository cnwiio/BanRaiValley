using System;
using System.Collections;
using UnityEngine;
using Lean.Pool;
using UnityEngine.Serialization;

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

public class AIBrain : MonoBehaviour, IPoolable
{
    [Header("Data")] 
    [SerializeField] private AIData data;

    
    [Header("Scripts")]
    [SerializeField] private AIMovement movement;
    [SerializeField] private AIDetection detection;
    [SerializeField] private AIAttack attack;
    [SerializeField] private AIAnimationController animator;
    [SerializeField] private AIHealth health;
    


    private const float MOVE_THRESHOLD = 1.5f;
    private const float HITBOX_LIFESPAN = 0.2f;

    private readonly WaitForSeconds chaseWaitInterval = new WaitForSeconds(0.25f);
    private WaitForSeconds stunTime;
    private readonly WaitForSeconds deSpawnTime = new WaitForSeconds(3);
    private Coroutine chaseCoroutine;
    private Coroutine movementCoroutine;
    private Coroutine stunCoroutine;
    private Coroutine dieCoroutine;

    private PlantAIState _currentState = PlantAIState.Idle;

    private PlantAIState currentState
    {
        get => _currentState;
        set
        {
            if (_currentState == value) return;
            
            ExitState(_currentState);
            EnterState(value);

            _currentState = value;
            Debug.Log(value);
        }
    }
    private PlantChaseState _currentChaseState = PlantChaseState.ChasePlayer;

    private PlantChaseState currentChaseState
    {
        get => _currentChaseState;
        set
        {
            // if (_currentChaseState == value) return;
            
            ExitChaseState(_currentChaseState);
            EnterChaseState(value);

            _currentChaseState = value;
            // Debug.Log(value);
        }
    }
    private void Awake()
    {
        stunTime = new WaitForSeconds(data.StunTime);
    }

    private void Initialize()
    {
        detection.Initialize(data.DetectionRange);
        movement.Initialize(transform.position, data.StopDistance, data.Speed, data.TurnSpeed, data.Acceleration);
        attack.Initialize(data.AttackCooldown, HITBOX_LIFESPAN);
        health.Initialize(data.HP);
        
        _lastKnowTargetPos = Vector3.zero;
    }

    private void OnEnable()
    {
        detection.OnTargetDetectedEvent += OnTargetDetected;
        detection.OnTargetLostEvent += OnTargetLost;

        attack.OnAttackEndEvent += OnAttackEnd;

        health.OnTakeDamageEvent += OnTakeDamage;
        health.OnDieEvent += OnDie;
    }

    private void OnDisable()
    {
        detection.OnTargetDetectedEvent -= OnTargetDetected;
        detection.OnTargetLostEvent -= OnTargetLost;

        attack.OnAttackEndEvent -= OnAttackEnd;

        health.OnTakeDamageEvent -= OnTakeDamage;
        health.OnDieEvent -= OnDie;
    }

    private void EnterState(PlantAIState state)
    {
        switch (state)
        {
            case PlantAIState.Awake:
                detection.EnableDetect(false);
                animator.PlayAwaken();
                break;
            case PlantAIState.Idle:
                break;
            case PlantAIState.Chase:
                movementCoroutine = StartCoroutine(AnimationSpeedCoroutine());
                break;
            case PlantAIState.Attack:
                movement.RotateToTarget();
                attack.StartAttack();
                animator.PlayAttack();
                break;
            case PlantAIState.Stun:
                movement.StopMoving();
                movement.RotateToTarget();
                attack.StopAttack();
                stunCoroutine = StartCoroutine(StunCoroutine());
                animator.PlayHit();
                break;
            case PlantAIState.Die:
                movement.StopMoving();
                attack.StopAttack();
                animator.PlayDie();
                dieCoroutine = StartCoroutine(DieCoroutine());
                break;
        }
    }

    private void ExitState(PlantAIState state)
    {
        switch (state)
        {
            case PlantAIState.Chase:
                if (chaseCoroutine != null)
                {
                    StopCoroutine(chaseCoroutine);
                    chaseCoroutine = null;
                }
                if (movementCoroutine != null)
                {
                    StopCoroutine(movementCoroutine);
                    movementCoroutine = null;
                }
                movement.StopMoving();
                animator.SetMoving(0f);
                break;
            case PlantAIState.Attack:
                break;
            case PlantAIState.Stun:
                if (stunCoroutine != null)
                {
                    StopCoroutine(stunCoroutine);
                    stunCoroutine = null;
                }
                break;
            case PlantAIState.Die:
                if (dieCoroutine != null)
                {
                    StopCoroutine(DieCoroutine());
                    dieCoroutine = null;
                }
                break;
        }
    }

    private void EnterChaseState(PlantChaseState state)
    {
        switch (state)
        {
            case PlantChaseState.ChasePlayer:
                chaseCoroutine = StartCoroutine(ChaseTargetCoroutine());
                break;
            case PlantChaseState.ChaseHome:
                movement.ReturnToStart();
                break;
        }
    }
    private void ExitChaseState(PlantChaseState state)
    {
        switch (state)
        {
            case PlantChaseState.ChasePlayer:
                if (chaseCoroutine != null)
                {
                    StopCoroutine(chaseCoroutine);
                    chaseCoroutine = null;
                }
                break;
            case PlantChaseState.ChaseHome:
                break;
        }
    }
    
    private void OnTargetDetected(Transform transform)
    {
        if (currentState == PlantAIState.Idle ||
            currentState == PlantAIState.Awake ||
            (currentState == PlantAIState.Chase && currentChaseState == PlantChaseState.ChaseHome))
        {
            currentState = PlantAIState.Chase;
            movement.BindTargetTransform(transform);
            currentChaseState = PlantChaseState.ChasePlayer;
        }
    }

    private void OnTargetLost()
    {
        if (currentState != PlantAIState.Chase) return;
        currentChaseState = PlantChaseState.ChaseHome;
    }

    private void OnAttackEnd()
    {
        if (currentState != PlantAIState.Attack) return;
        currentState = PlantAIState.Chase;
        if (detection.IsPlayerInSight)
        {
            currentChaseState = PlantChaseState.ChasePlayer;
        }
        else
        {
            currentChaseState = PlantChaseState.ChaseHome;
        }
    }

    public void OnAttackAnimationHit()
    {
        if (currentState != PlantAIState.Attack) return;
        attack.ExcuteAttack();
    }

    private void OnTakeDamage()
    {
        if (currentState == PlantAIState.Die) return;
        if (currentState == PlantAIState.Stun)
        {
            StopCoroutine(stunCoroutine);
            stunCoroutine = null;
            stunCoroutine = StartCoroutine(StunCoroutine());
            movement.RotateToTarget();
            animator.PlayHit();
        }
        currentState = PlantAIState.Stun;
    }

    private void OnDie()
    {
        currentState = PlantAIState.Die;
    }

    public void OnAwakeAnimationEnd()
    {
        currentState = PlantAIState.Idle;
        detection.EnableDetect(true);
    }
    
    private float _distanceFromLastPos;
    private float _distanceFromPlayer;
    private Vector3 _lastKnowTargetPos;
    private IEnumerator ChaseTargetCoroutine()
    {
        if (movement.targetTranform == null)
        {
            Debug.LogError("no Target transform to chasing");
            yield break;
        }
        // var waitInterval = new WaitForSeconds(CHASE_UPDATE_INTERVAL_SEC);
        var target = movement.targetTranform;
        while (currentState == PlantAIState.Chase)
        {
            _distanceFromPlayer = Vector3.Distance(transform.position, target.position);
            if (_distanceFromPlayer <= data.AttackRange)
            {
                if (attack.CanAttack)
                    currentState = PlantAIState.Attack;
            }
            else
            {
                _distanceFromLastPos = Vector3.Distance(target.position, _lastKnowTargetPos);
                if (_distanceFromLastPos > MOVE_THRESHOLD || _lastKnowTargetPos == Vector3.zero)
                {
                    _lastKnowTargetPos = target.position;
                    movement.MoveTo(target.position);
                }
            }
            yield return chaseWaitInterval;
        }
    }
    
    private IEnumerator AnimationSpeedCoroutine()
    {
        while (true)
        {
            if (currentChaseState == PlantChaseState.ChaseHome)
            {
                if (movement.IsReachDestination)
                {
                    currentState = PlantAIState.Idle;
                }
            }
            animator.SetMoving(movement.Speed);
            yield return chaseWaitInterval;
        }
    }

    private IEnumerator StunCoroutine()
    {
        yield return stunTime;
        currentState = PlantAIState.Chase;
        if (detection.IsPlayerInSight)
        {
            currentChaseState = PlantChaseState.ChasePlayer;
        }
        else
        {
            currentChaseState = PlantChaseState.ChaseHome;
        }
    }

    private IEnumerator DieCoroutine()
    {
        yield return deSpawnTime;
        LeanPool.Despawn(this);
        dieCoroutine = null;
    }

    public void OnSpawn()
    {
        Initialize();
        currentState = PlantAIState.Awake;
    }

    public void OnDespawn()
    {
        animator.Reset();
        // currentState = PlantAIState.Idle;
    }
}
