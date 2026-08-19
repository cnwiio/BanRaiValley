using System;
using UnityEngine;

/// <summary>
/// Handles target sensing for a Plant AI entity using a <see cref="SphereCollider"/>
/// trigger zone. Detects and tracks targets via <c>OnTriggerEnter</c>/<c>OnTriggerExit</c>
/// callbacks — no <c>Update</c> polling is used.
/// </summary>
public class PlantPerception : MonoBehaviour
{
    #region Serialized Fields

    [Tooltip("SphereCollider configured as a trigger zone for aggro detection.")]
    [SerializeField] private SphereCollider _aggroTrigger;

    [Tooltip("LayerMask filtering which layers are valid targets (e.g. Player).")]
    [SerializeField] private LayerMask _targetLayer;

    #endregion

    #region Public Properties

    /// <summary>
    /// The Transform of the currently tracked target, or <c>null</c> if no target is in range.
    /// </summary>
    public Transform CurrentTarget { get; private set; }

    /// <summary>
    /// Returns <c>true</c> when a valid target is currently being tracked.
    /// </summary>
    public bool HasTarget => CurrentTarget != null;

    #endregion

    #region Events

    /// <summary>Raised when a valid target enters the aggro trigger zone.</summary>
    public event Action<Transform> OnTargetDetected;

    /// <summary>Raised when the currently tracked target leaves the aggro zone or is cleared.</summary>
    public event Action OnTargetLost;

    #endregion

    #region Public Methods

    /// <summary>
    /// Configures the aggro trigger radius. Call once from the owning controller
    /// after instantiation.
    /// </summary>
    /// <param name="aggroRadiusM">Detection radius in metres.</param>
    public void Initialize(float aggroRadiusM)
    {
        _aggroTrigger.radius = aggroRadiusM;
        _aggroTrigger.isTrigger = true;
    }

    /// <summary>
    /// Enables or disables the aggro trigger collider.
    /// Used to suspend perception during dormancy, death, or awakening.
    /// </summary>
    /// <param name="isActive"><c>true</c> to enable detection, <c>false</c> to disable.</param>
    public void SetPerceptionActive(bool isActive)
    {
        _aggroTrigger.enabled = isActive;
    }

    /// <summary>
    /// Manually clears the current target and fires <see cref="OnTargetLost"/>.
    /// Used when forcing a target reset (e.g. on state transitions).
    /// </summary>
    public void ClearTarget()
    {
        if (CurrentTarget == null) return;

        CurrentTarget = null;
        OnTargetLost?.Invoke();
    }

    #endregion

    #region Unity Trigger Callbacks

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger Enter");
        if (CurrentTarget != null) return;
        if (!IsInTargetLayer(other.gameObject)) return;

        CurrentTarget = other.transform;
        OnTargetDetected?.Invoke(CurrentTarget);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform != CurrentTarget) return;

        CurrentTarget = null;
        OnTargetLost?.Invoke();
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Checks whether the given GameObject belongs to the configured target layer mask.
    /// </summary>
    /// <param name="target">The GameObject to test.</param>
    /// <returns><c>true</c> if the GameObject's layer matches <see cref="_targetLayer"/>.</returns>
    private bool IsInTargetLayer(GameObject target)
    {
        return ((1 << target.layer) & _targetLayer) != 0;
    }

    #endregion
}
