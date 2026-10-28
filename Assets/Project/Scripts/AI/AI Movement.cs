using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Concrete NavMesh-based locomotion and rotation component for plant monsters.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class AIMovement : AIMovementBase
{
    #region Serialized Fields
    [Header("Navigation")]
    [Tooltip("Reference to the NavMeshAgent handling pathfinding.")]
    [SerializeField] private NavMeshAgent agent;

    [Tooltip("Target transform being pursued.")]
    [HideInInspector] [SerializeField] public Transform targetTranform;
    #endregion

    #region Fields
    private Vector3 _startPos;
    private float _stopDistance;
    #endregion

    #region Properties
    /// <summary>Gets the current movement speed magnitude of the agent.</summary>
    public override float Speed => agent != null ? agent.velocity.magnitude : 0f;

    /// <summary>Gets whether the plant has completed its path or stopped.</summary>
    public override bool IsReachDestination => agent != null && !agent.hasPath;

    /// <summary>Gets the bound target transform.</summary>
    public override Transform TargetTransform => targetTranform;
    #endregion

    #region Unity Messages
    private void Awake()
    {
        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
        }
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Initializes movement parameters from configuration data.
    /// </summary>
    /// <param name="startPosition">Home coordinate to return to.</param>
    /// <param name="data">Data asset containing speed, angular speed, and acceleration.</param>
    public override void Initialize(Vector3 startPosition, AIData data)
    {
        _startPos = startPosition;
        if (data != null)
        {
            _stopDistance = data.StopDistance;
            if (agent != null)
            {
                agent.speed = data.Speed;
                agent.angularSpeed = data.TurnSpeed;
                agent.acceleration = data.Acceleration;
            }
        }
    }

    /// <summary>
    /// Backwards-compatible initialization overload accepting discrete movement parameters.
    /// </summary>
    public void Initialize(Vector3 startPosition, float stoppingDistance, float speed, float turnSpeed, float acceleration)
    {
        _startPos = startPosition;
        _stopDistance = stoppingDistance;
        if (agent != null)
        {
            agent.speed = speed;
            agent.angularSpeed = turnSpeed;
            agent.acceleration = acceleration;
        }
    }

    /// <summary>Binds a target transform for chasing and orientation.</summary>
    public override void BindTargetTransform(Transform target)
    {
        targetTranform = target;
    }

    /// <summary>Unbinds the current target transform.</summary>
    public override void UnBindTargetTransform()
    {
        targetTranform = null;
    }

    /// <summary>Navigates directly towards the bound target transform.</summary>
    public void MoveToTarget()
    {
        if (targetTranform != null)
        {
            MoveTo(targetTranform.position);
        }
    }

    /// <summary>Commands the movement system to navigate toward a destination coordinate.</summary>
    public override void MoveTo(Vector3 targetPos)
    {
        if (agent == null || !agent.isActiveAndEnabled || !agent.isOnNavMesh)
        {
            Debug.LogWarning("[AIMovement] Cannot set Destination - Agent is null, inactive, or not on NavMesh.");
            return;
        }

        agent.stoppingDistance = _stopDistance;
        agent.isStopped = false;
        agent.SetDestination(targetPos);
    }

    /// <summary>Commands the plant to navigate back to its starting/home position.</summary>
    public override void ReturnToStart()
    {
        if (agent == null || !agent.isActiveAndEnabled || !agent.isOnNavMesh)
        {
            return;
        }

        agent.stoppingDistance = 0f;
        agent.isStopped = false;
        agent.SetDestination(_startPos);
    }

    /// <summary>Stops all active movement immediately.</summary>
    public override void StopMoving()
    {
        if (agent == null || !agent.isActiveAndEnabled || !agent.isOnNavMesh)
        {
            return;
        }

        agent.isStopped = true;
        agent.ResetPath();
    }

    /// <summary>Rotates the plant horizontally towards its currently bound target.</summary>
    public override void RotateToTarget()
    {
        if (targetTranform == null)
        {
            return;
        }

        Vector3 targetPos = targetTranform.position;
        targetPos.y = transform.position.y;

        Vector3 direction = targetPos - transform.position;
        if (direction.sqrMagnitude < 0.001f)
        {
            return;
        }

        transform.rotation = Quaternion.LookRotation(direction);
    }
    #endregion
}
