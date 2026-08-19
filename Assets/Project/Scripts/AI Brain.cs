using System;
using System.Collections;
using UnityEngine;
using Lean.Pool;
using UnityEngine.Serialization;

public enum PlantAIState
{
    Idle,
    Chase,
    Attack
}

public enum PlantChaseState
{
    ChasePlayer,
    ChaseHome
}

public class AIBrain : MonoBehaviour, IPoolable
{
    [SerializeField] private AIMovement movement;
    [SerializeField] private AIDetection detection;
    [SerializeField] private AIAttack attack;
    [SerializeField] private AIAnimationController animator;


    private const float MOVE_THRESHOLD = 1.5f;
    private const float STOP_DISTANCE = 3f;
    private const float ATTACK_RANGE = 5;
    private const float ATTACK_COOLDOWN = 2;
    private const float HITBOX_LIFESPAN = 0.2f;
    
    private Coroutine chaseCoroutine;
    private Coroutine movementCoroutine;

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
            if (_currentChaseState == value) return;
            
            ExitChaseState(_currentChaseState);
            EnterChaseState(value);

            _currentChaseState = value;
            // Debug.Log(value);
        }
    }
    private void Awake()
    {
        movement.Initialize(transform.position, STOP_DISTANCE);
        detection.Initialize(movement);
        attack.Initialize(ATTACK_COOLDOWN, HITBOX_LIFESPAN );
    }

    private void OnEnable()
    {
        detection.OnTargetDetectedEvent += OnTargetDetected;
        detection.OnTargetLostEvent += OnTargetLost;

        attack.OnAttackEndEvent += OnAttackEnd;
    }

    private void OnDisable()
    {
        detection.OnTargetDetectedEvent -= OnTargetDetected;
        detection.OnTargetLostEvent -= OnTargetLost;

        attack.OnAttackEndEvent -= OnAttackEnd;
    }

    private void EnterState(PlantAIState state)
    {
        switch (state)
        {
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
        }
    }

    private void EnterChaseState(PlantChaseState state)
    {
        switch (state)
        {
            case PlantChaseState.ChasePlayer:
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
            (currentState == PlantAIState.Chase && currentChaseState == PlantChaseState.ChaseHome))
        {
            currentState = PlantAIState.Chase;
            currentChaseState = PlantChaseState.ChasePlayer;
            movement.BindTargetTransform(transform);
            chaseCoroutine = StartCoroutine(ChaseTargetCoroutine());
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
            chaseCoroutine = StartCoroutine(ChaseTargetCoroutine());
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
    
    private float _distanceFromLastPos;
    private float _distanceFromPlayer;
    private Vector3 _lastKnowTargetPos;
    private const float CHASE_UPDATE_INTERVAL_SEC = 0.25f;
    private IEnumerator ChaseTargetCoroutine()
    {
        if (movement.targetTranform == null)
        {
            Debug.LogError("no Target transform to chasing");
            yield break;
        }
        var waitInterval = new WaitForSeconds(CHASE_UPDATE_INTERVAL_SEC);
        var target = movement.targetTranform;
        while (currentState == PlantAIState.Chase)
        {
            _distanceFromPlayer = Vector3.Distance(transform.position, target.position);
            if (_distanceFromPlayer <= ATTACK_RANGE)
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
            yield return waitInterval;
        }
    }
    
    private IEnumerator AnimationSpeedCoroutine()
    {
        var waitInterval = new WaitForSeconds(CHASE_UPDATE_INTERVAL_SEC);
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
            yield return waitInterval;
        }
    }

    public void OnSpawn()
    {
        throw new NotImplementedException();
    }

    public void OnDespawn()
    {
        throw new NotImplementedException();
    }
}
