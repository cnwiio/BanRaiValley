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

public class AIBrain : MonoBehaviour, IPoolable
{
    [SerializeField] private AIMovement movement;
    [SerializeField] private AIDetection detection;
    [SerializeField] private AIAttack attack;
    

    private const float MOVE_THRESHOLD = 1.5f;
    private const float STOP_DISTANCE = 3f;
    private const float ATTACK_RANGE = 5;
    private const float ATTACK_COOLDOWN = 1;
    private const float HITBOX_LIFESPAN = 0.2f;
    
    private Coroutine chaseCoroutine;

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
                movement.ReturnToStart();
                break;
            case PlantAIState.Attack:
                movement.StopMoving();
                attack.StartAttack();
                break;
        }
    }

    private void ExitState(PlantAIState state)
    {
        switch (state)
        {
            case PlantAIState.Chase :
                StopCoroutine(chaseCoroutine);
                chaseCoroutine = null;
                break;
            case PlantAIState.Attack:
                break;
        }
    }
    
    private void OnTargetDetected(Transform transform)
    {
        if (currentState != PlantAIState.Idle) return;
        currentState = PlantAIState.Chase;
        chaseCoroutine = StartCoroutine(ChaseTargetCoroutine(transform));
    }

    private void OnTargetLost()
    {
        if (currentState != PlantAIState.Chase) return;
        currentState = PlantAIState.Idle;
    }

    private void OnAttackEnd()
    {
        if (currentState != PlantAIState.Attack) return;
        currentState = detection.IsPlayerInSight ? PlantAIState.Chase : PlantAIState.Idle;
    }
    
    private float _distanceFromLastPos;
    private float _distanceFromPlayer;
    private Vector3 _lastKnowTargetPos;
    private const float CHASE_UPDATE_INTERVAL_SEC = 0.25f;
    private IEnumerator ChaseTargetCoroutine(Transform target)
    {
        var waitInterval = new WaitForSeconds(CHASE_UPDATE_INTERVAL_SEC);
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
                if (_distanceFromLastPos > MOVE_THRESHOLD)
                {
                    _lastKnowTargetPos = target.position;
                    movement.MoveTo(target.position);
                }
            }
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
