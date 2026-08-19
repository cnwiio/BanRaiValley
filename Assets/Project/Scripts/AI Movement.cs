using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

[RequireComponent(typeof(NavMeshAgent))]
public class AIMovement : MonoBehaviour, IWalkable
{
    [SerializeField] private NavMeshAgent agent;

    private Vector3 startPos;
    private float stopDistance;
    public void Initialize(Vector3 startPosition, float stoppingDistance)
    {
        startPos = startPosition;
        stopDistance = stoppingDistance;
    }
    
    private Vector3 _lastKnowTargetPos;
    private float _distanceFromLastPos;
    private const float MOVE_THRESHOLD = 2;
    private float _updateTimer;
    private const float UPDATE_INTERVAL = 0.25f;
    public void MoveTo(Vector3 targetPos)
    {
        if (!agent.isActiveAndEnabled || !agent.isOnNavMesh)
        {
            Debug.LogWarning("Can not set Destination");
            return;
        }
        
        agent.stoppingDistance = stopDistance;
        agent.isStopped = false;
        agent.SetDestination(targetPos);
    }
    
    public void ReturnToStart()
    {
        if (!agent.isActiveAndEnabled || !agent.isOnNavMesh)
        {
            return;
        }
        
        agent.stoppingDistance = 0;
        agent.SetDestination(startPos);
    }

    public void StopMoving()
    {
        if (!agent.isActiveAndEnabled || !agent.isOnNavMesh)
        {
            return;
        }
        
        agent.isStopped = true;
        agent.ResetPath();
    }
}
