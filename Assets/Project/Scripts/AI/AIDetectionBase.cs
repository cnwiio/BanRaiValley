using System;
using UnityEngine;

/// <summary>
/// Abstract base class for plant perception and target detection.
/// Concrete implementations handle target sensing (e.g. sphere trigger, line-of-sight raycast).
/// </summary>
public abstract class AIDetectionBase : MonoBehaviour
{
    #region Properties
    /// <summary>Gets whether a valid player target is currently in perception range.</summary>
    public abstract bool IsPlayerInSight { get; }

    /// <summary>Gets the transform of the currently spotted target, if any.</summary>
    public abstract Transform TargetTransform { get; }
    #endregion

    #region Events
    /// <summary>Event raised when a target enters detection range.</summary>
    public abstract event Action<Transform> OnTargetDetectedEvent;

    /// <summary>Event raised when the target leaves detection range.</summary>
    public abstract event Action OnTargetLostEvent;
    #endregion

    #region Public Methods
    /// <summary>Initializes detection parameters.</summary>
    /// <param name="detectionRange">Radius or range of target perception.</param>
    public abstract void Initialize(float detectionRange);

    /// <summary>Enables or disables target sensing.</summary>
    /// <param name="isEnabled">True to enable detection; false to turn off perception.</param>
    public abstract void EnableDetect(bool isEnabled);
    #endregion
}
