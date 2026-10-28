using UnityEngine;

/// <summary>
/// Abstract base class for plant locomotion and rotation.
/// Concrete implementations handle specific navigation strategies (e.g. NavMesh, rooted, hopping).
/// </summary>
public abstract class AIMovementBase : MonoBehaviour
{
    #region Properties
    /// <summary>Gets the current movement speed of the plant.</summary>
    public abstract float Speed { get; }

    /// <summary>Gets whether the plant has reached its current navigation destination.</summary>
    public abstract bool IsReachDestination { get; }

    /// <summary>Gets the target transform being chased or focused.</summary>
    public abstract Transform TargetTransform { get; }
    #endregion

    #region Public Methods
    /// <summary>Initializes movement parameters from configuration data.</summary>
    /// <param name="startPosition">Home / spawn position to return to when losing target.</param>
    /// <param name="data">Configuration data asset containing speed, stopping distance, etc.</param>
    public abstract void Initialize(Vector3 startPosition, AIData data);

    /// <summary>Binds a target transform for chasing and orientation.</summary>
    /// <param name="target">Target transform to pursue.</param>
    public abstract void BindTargetTransform(Transform target);

    /// <summary>Unbinds the current target transform.</summary>
    public abstract void UnBindTargetTransform();

    /// <summary>Commands the movement system to navigate toward a destination coordinate.</summary>
    /// <param name="targetPos">World destination position.</param>
    public abstract void MoveTo(Vector3 targetPos);

    /// <summary>Commands the plant to navigate back to its starting/home position.</summary>
    public abstract void ReturnToStart();

    /// <summary>Stops all active movement immediately.</summary>
    public abstract void StopMoving();

    /// <summary>Rotates the plant towards its currently bound target.</summary>
    public abstract void RotateToTarget();
    #endregion
}
