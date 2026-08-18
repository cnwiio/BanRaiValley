using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Action-layer component that encapsulates all NavMeshAgent locomotion for a plant AI.
/// Provides safe wrappers for destination setting, stopping, resuming, and smooth rotation
/// without exposing the raw NavMeshAgent to higher-level state logic.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class PlantMovement : MonoBehaviour
{
    #region Serialized Fields

    [Header("Navigation")]
    [Tooltip("Reference to the NavMeshAgent on this GameObject. Auto-populated via RequireComponent.")]
    [SerializeField] private NavMeshAgent _navMeshAgent;

    #endregion

    #region Fields

    private float _rotationSpeedDeg;

    #endregion

    #region Unity Messages

    private void Reset()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Configures the NavMeshAgent speed, stopping distance, and rotation speed.
    /// Must be called once by the owning controller before any movement requests.
    /// </summary>
    /// <param name="speedUps">Movement speed in Unity units per second.</param>
    /// <param name="stoppingDistM">Distance in metres at which the agent stops before reaching its destination.</param>
    /// <param name="rotationSpeedDeg">Smooth-rotation speed in degrees per second used by <see cref="RotateTowards"/>.</param>
    public void Initialize(float speedUps, float stoppingDistM, float rotationSpeedDeg)
    {
        _navMeshAgent.speed           = speedUps;
        _navMeshAgent.stoppingDistance = stoppingDistM;
        _rotationSpeedDeg             = rotationSpeedDeg;
    }

    /// <summary>
    /// Orders the agent to navigate to the specified world position.
    /// Silently skips the request if the agent is disabled or not on a NavMesh.
    /// </summary>
    /// <param name="targetPosition">Desired destination in world space.</param>
    public void SetDestination(Vector3 targetPosition)
    {
        if (!_navMeshAgent.isActiveAndEnabled || !_navMeshAgent.isOnNavMesh)
        {
            return;
        }

        _navMeshAgent.isStopped = false;
        _navMeshAgent.SetDestination(targetPosition);
    }

    /// <summary>
    /// Immediately halts the agent by resetting its path and marking it as stopped.
    /// </summary>
    public void StopMovement()
    {
        if (!_navMeshAgent.isActiveAndEnabled || !_navMeshAgent.isOnNavMesh)
        {
            return;
        }

        _navMeshAgent.ResetPath();
        _navMeshAgent.isStopped = true;
    }

    /// <summary>
    /// Clears the stopped flag, allowing the agent to resume following its current or next destination.
    /// </summary>
    public void ResumeMovement()
    {
        if (!_navMeshAgent.isActiveAndEnabled || !_navMeshAgent.isOnNavMesh)
        {
            return;
        }

        _navMeshAgent.isStopped = false;
    }

    /// <summary>
    /// Smoothly rotates the GameObject toward the target world position using
    /// <see cref="_rotationSpeedDeg"/> degrees per second. Ignores the Y-axis delta
    /// to prevent unwanted tilting on uneven terrain.
    /// </summary>
    /// <param name="targetWorldPos">World-space position to face toward.</param>
    public void RotateTowards(Vector3 targetWorldPos)
    {
        Vector3 direction = targetWorldPos - transform.position;
        // Zero out vertical component so the plant never pitches up or down.
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
        {
            return;
        }

        Quaternion targetRotation  = Quaternion.LookRotation(direction);
        float      maxDegreeDelta  = _rotationSpeedDeg * Time.deltaTime;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, maxDegreeDelta);
    }

    /// <summary>
    /// Returns <c>true</c> when the agent has reached or is within stopping distance of its destination
    /// and is not waiting for a path to be computed.
    /// </summary>
    /// <returns><c>true</c> if the agent is at its destination; otherwise <c>false</c>.</returns>
    public bool IsAtDestination()
    {
        if (_navMeshAgent.pathPending)
        {
            return false;
        }

        return _navMeshAgent.remainingDistance <= _navMeshAgent.stoppingDistance;
    }

    #endregion
}
