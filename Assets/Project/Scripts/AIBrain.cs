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

    private const float ATTACK_RANGE = 5;
    
    private Coroutine chaseCoroutine;

    private PlantAIState _currentState = PlantAIState.Idle;

    private PlantAIState currentState
    {
        get => _currentState;
        set
        {
            EnterState(value);
            EnterState(_currentState);

            _currentState = value;
        }
    }
    private void Awake()
    {
        movement.Initialize(transform.position, 3);
        detection.Initialize(movement);
    }

    private void OnEnable()
    {
        detection.OnTargetDetectedEvent += OnTargetDetected;
        detection.OnTargetLostEvent += OnTargetLost;
    }

    private void OnDisable()
    {
        detection.OnTargetDetectedEvent -= OnTargetDetected;
        detection.OnTargetLostEvent -= OnTargetLost;
    }

    private void EnterState(PlantAIState state)
    {
        switch (state)
        {
            case PlantAIState.Attack:
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

    private float _distanceFromLastPos;
    private float _distanceFromPlayer;
    private Vector3 _lastKnowTargetPos;
    private const float MOVE_THRESHOLD = 2;
    private const float CHASE_UPDATE_INTERVAL_SEC = 0.25f;
    private IEnumerator ChaseTargetCoroutine(Transform target)
    {
        var waitInterval = new WaitForSeconds(CHASE_UPDATE_INTERVAL_SEC);
        while (currentState == PlantAIState.Chase)
        {
            _distanceFromPlayer = Vector3.Distance(transform.position, target.position);
            if (_distanceFromPlayer <= ATTACK_RANGE)
            {
                // currentState == PlantAIState.Attack;
                
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

    private void OnTargetDetected(Transform transform)
    {
        currentState = PlantAIState.Chase;
        chaseCoroutine = StartCoroutine(ChaseTargetCoroutine(transform));
    }

    private void OnTargetLost()
    {
        currentState = PlantAIState.Idle;
        movement.ReturnToStart();
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
