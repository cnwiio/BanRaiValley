using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

[RequireComponent(typeof(NavMeshAgent))]
public class AIMovement : MonoBehaviour
{
    [SerializeField] private NavMeshAgent agent;

    private Vector3 startPos;
    private float stopDistance;
    [HideInInspector] public Transform targetTranform;

    public float Speed => agent.velocity.magnitude;
    public bool IsReachDestination => !agent.hasPath;
    public void Initialize(Vector3 startPosition, float stoppingDistance, float speed, float turnSpeed, float acceleration)
    {
        startPos = startPosition;
        stopDistance = stoppingDistance;
        agent.speed = speed;
        agent.angularSpeed = turnSpeed;
        agent.acceleration = acceleration;
    }
    
    public void BindTargetTransform(Transform transform)
    {
        targetTranform = transform;
    }

    public void UnBindTargetTransform()
    {
        targetTranform = null;
    }

    public void MoveToTarget()
    {
        if (!agent.isActiveAndEnabled || !agent.isOnNavMesh)
        {
            Debug.LogWarning("Can not set Destination");
            return;
        }
        
        agent.stoppingDistance = stopDistance;
        agent.isStopped = false;
        agent.SetDestination(targetTranform.position);
    }
        
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
    
    public void RotateToTarget()
    {
        Vector3 targetPos = targetTranform.position;
        // Zero out vertical component so the plant never pitches up or down.
        targetPos.y = 0f;

        if (targetPos.sqrMagnitude < 0.001f)
        {
            return;
        }
        transform.LookAt(targetPos);
    }
}
