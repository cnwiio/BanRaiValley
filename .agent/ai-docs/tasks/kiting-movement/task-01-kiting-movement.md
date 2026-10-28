# Task 01: Implement `AIKitingMovement.cs`

## 1. Task Goal
Implement `AIKitingMovement.cs`, a specialized locomotive component inheriting from `AIMovementBase`. When the player closes in beyond `StopDistance`, the monster automatically backs away at a reduced speed while smoothly keeping its gaze locked directly on the player. When within the buffer sweet spot, it holds ground; when outside the buffer, it approaches.

## 2. Task Information
- **System**: Plant AI System (Locomotion)
- **Parent Plan**: [kiting-movement-plan.md](../../plan/kiting-movement-plan.md)
- **Target File(s)**:
  - `Assets/Project/Scripts/AI/AIKitingMovement.cs`
- **Dependencies / Prerequisites**:
  - `AIMovementBase.cs` (Task 01 of `plant-ai-framework`)
- **Applicable Rules**:
  - `architecture-guide.md`: Section 3 (Composition over inheritance, max depth 2: MonoBehaviour -> AIMovementBase -> AIKitingMovement), Section 10 (AI Perception/Decision/Action separation).
  - `code-style-rule.md`: Standard `#region` order, XML documentation on all public members, `[Header]` and `[Tooltip]` on serialized fields.
  - `function-rule.md`: Strict Command vs Query separation.
  - `naming-variable-rule.md`: Modal verbs on booleans (`_isRetreating`), units in variable names where appropriate.

## 3. What To Do (Step-by-Step Instructions)

Create `Assets/Project/Scripts/AI/AIKitingMovement.cs`:

```csharp
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Locomotive strategy for ranged plant monsters that kite / back away from approaching targets.
/// When the target is closer than stopping distance, the monster slowly walks backward while keeping
/// its gaze locked on the target.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class AIKitingMovement : AIMovementBase
{
    #region Serialized Fields
    [Header("Agent Configuration")]
    [Tooltip("Reference to the NavMeshAgent driving navigation.")]
    [SerializeField] private NavMeshAgent _agent;

    [Header("Kiting Parameters")]
    [Tooltip("Speed multiplier applied to base speed when walking backward away from the target.")]
    [Range(0.1f, 1f)]
    [SerializeField] private float _backwardSpeedMultiplier = 0.5f;

    [Tooltip("Deadband buffer added to stopDistance to prevent jittering at the retreat threshold (units).")]
    [SerializeField] private float _retreatBuffer = 1.0f;

    [Tooltip("Distance sampled backward along the retreat vector on the NavMesh per retreat step (units).")]
    [SerializeField] private float _retreatStepDistance = 2.5f;

    [Tooltip("Maximum distance to search for a valid NavMesh point when sampling retreat destinations.")]
    [SerializeField] private float _navMeshSampleRadius = 1.5f;
    #endregion

    #region Properties
    /// <summary>Gets the current movement speed magnitude of the agent.</summary>
    public override float Speed => _agent != null ? _agent.velocity.magnitude : 0f;

    /// <summary>Gets whether the agent has reached its current navigation destination.</summary>
    public override bool IsReachDestination => _agent != null && !_agent.hasPath;

    /// <summary>Gets the target transform currently bound for navigation and tracking.</summary>
    public override Transform TargetTransform => _targetTransform;

    /// <summary>Gets whether the monster is currently executing a backward retreat away from the target.</summary>
    public bool IsRetreating => _isRetreating;
    #endregion

    #region Fields
    private Vector3 _startPosition;
    private float _stopDistance;
    private float _baseSpeed;
    private float _turnSpeed;
    private Transform _targetTransform;
    private bool _isRetreating;
    #endregion

    #region Unity Messages
    private void Awake()
    {
        if (_agent == null)
        {
            _agent = GetComponent<NavMeshAgent>();
        }
    }

    private void Update()
    {
        // When retreating, NavMeshAgent automatic rotation is disabled to allow smooth manual facing
        if (_isRetreating && _targetTransform != null)
        {
            RotateToTarget();
        }
    }
    #endregion

    #region Public Methods
    /// <summary>Initializes navigation parameters from configuration data.</summary>
    /// <param name="startPosition">Home position to return to when losing target.</param>
    /// <param name="data">Data asset containing speed, stopping distance, turn speed, and acceleration.</param>
    public override void Initialize(Vector3 startPosition, AIData data)
    {
        _startPosition = startPosition;
        _stopDistance = data.StopDistance;
        _baseSpeed = data.Speed;
        _turnSpeed = data.TurnSpeed;

        if (_agent != null)
        {
            _agent.speed = _baseSpeed;
            _agent.angularSpeed = _turnSpeed;
            _agent.acceleration = data.Acceleration;
            _agent.stoppingDistance = _stopDistance;
        }
    }

    /// <summary>Binds a target transform for pursuit and orientation.</summary>
    /// <param name="target">Target transform to pursue.</param>
    public override void BindTargetTransform(Transform target)
    {
        _targetTransform = target;
    }

    /// <summary>Unbinds the current target transform and resets retreat state.</summary>
    public override void UnBindTargetTransform()
    {
        _targetTransform = null;
        _isRetreating = false;
        if (_agent != null && _agent.isActiveAndEnabled)
        {
            _agent.updateRotation = true;
        }
    }

    /// <summary>
    /// Commands the movement system to evaluate target proximity and navigate accordingly:
    /// - Retreats slowly backward if target is closer than stopDistance.
    /// - Holds position if target is within [stopDistance, stopDistance + retreatBuffer].
    /// - Moves forward if target is beyond stopDistance + retreatBuffer.
    /// </summary>
    /// <param name="targetPos">World destination position (usually target position).</param>
    public override void MoveTo(Vector3 targetPos)
    {
        if (_agent == null || !_agent.isActiveAndEnabled || !_agent.isOnNavMesh)
        {
            return;
        }

        float distanceToTarget = _targetTransform != null
            ? Vector3.Distance(transform.position, _targetTransform.position)
            : Vector3.Distance(transform.position, targetPos);

        // Zone 1: Too close -> slowly walk backward facing player
        if (distanceToTarget < _stopDistance)
        {
            ExecuteRetreat();
            return;
        }

        // Zone 2: Sweet spot -> hold ground and face player
        if (distanceToTarget <= _stopDistance + _retreatBuffer)
        {
            HoldGround();
            return;
        }

        // Zone 3: Beyond sweet spot -> advance normally
        AdvanceToward(targetPos);
    }

    /// <summary>Commands the plant to navigate back to its home spawn point.</summary>
    public override void ReturnToStart()
    {
        if (_agent == null || !_agent.isActiveAndEnabled || !_agent.isOnNavMesh)
        {
            return;
        }

        _isRetreating = false;
        _agent.updateRotation = true;
        _agent.speed = _baseSpeed;
        _agent.stoppingDistance = 0f;
        _agent.isStopped = false;
        _agent.SetDestination(_startPosition);
    }

    /// <summary>Stops all active movement immediately.</summary>
    public override void StopMoving()
    {
        if (_agent == null || !_agent.isActiveAndEnabled || !_agent.isOnNavMesh)
        {
            return;
        }

        _isRetreating = false;
        _agent.isStopped = true;
        _agent.ResetPath();
    }

    /// <summary>Smoothly rotates the plant towards its currently bound target on the horizontal plane.</summary>
    public override void RotateToTarget()
    {
        if (_targetTransform == null)
        {
            return;
        }

        Vector3 direction = _targetTransform.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, _turnSpeed * Time.deltaTime);
    }
    #endregion

    #region Private Methods
    private void ExecuteRetreat()
    {
        _isRetreating = true;
        _agent.updateRotation = false;
        _agent.speed = _baseSpeed * _backwardSpeedMultiplier;
        _agent.stoppingDistance = 0f;
        _agent.isStopped = false;

        Vector3 retreatDirection = _targetTransform != null
            ? (transform.position - _targetTransform.position)
            : -transform.forward;
        retreatDirection.y = 0f;

        if (retreatDirection.sqrMagnitude < 0.001f)
        {
            retreatDirection = -transform.forward;
        }
        retreatDirection.Normalize();

        Vector3 desiredBackwardPoint = transform.position + retreatDirection * _retreatStepDistance;

        if (NavMesh.SamplePosition(desiredBackwardPoint, out NavMeshHit hit, _navMeshSampleRadius, NavMesh.AllAreas))
        {
            _agent.SetDestination(hit.position);
        }
        else
        {
            // Backed against an obstacle/wall: stop moving to prevent stutter
            _agent.isStopped = true;
            _agent.ResetPath();
        }
    }

    private void HoldGround()
    {
        _isRetreating = false;
        _agent.updateRotation = false;
        _agent.isStopped = true;
        _agent.ResetPath();
        RotateToTarget();
    }

    private void AdvanceToward(Vector3 targetPos)
    {
        _isRetreating = false;
        _agent.updateRotation = true;
        _agent.speed = _baseSpeed;
        _agent.stoppingDistance = _stopDistance;
        _agent.isStopped = false;
        _agent.SetDestination(targetPos);
    }
    #endregion
}
```

## 4. Verification & Testing Checklist
- [ ] Compiles with zero errors in Unity 6.3.
- [ ] In Inspector, `_backwardSpeedMultiplier`, `_retreatBuffer`, `_retreatStepDistance` have valid defaults and tooltips.
- [ ] When player approaches inside `StopDistance`, monster moves backward while `transform` continues facing the player.
- [ ] When player stands within `StopDistance` to `StopDistance + _retreatBuffer`, monster holds position with zero jitter.
- [ ] When player retreats beyond `StopDistance + _retreatBuffer`, monster moves forward normally.
- [ ] When backed into a wall, monster cleanly stops rather than jittering into the collider.
