using System;
using UnityEngine;

/// <summary>
/// Concrete trigger-based player perception component for plant monsters.
/// </summary>
public class AIDetection : AIDetectionBase
{
    #region Serialized Fields
    [Header("Detection")]
    [Tooltip("Sphere collider trigger volume defining the perception radius.")]
    [SerializeField] private SphereCollider _collider;
    #endregion

    #region Fields
    private Transform _playerTransform;
    #endregion

    #region Properties
    /// <summary>Gets whether a valid player target is currently within detection range.</summary>
    public override bool IsPlayerInSight => _playerTransform != null;

    /// <summary>Gets the transform of the currently spotted target, if any.</summary>
    public override Transform TargetTransform => _playerTransform;
    #endregion

    #region Events
    /// <summary>Event raised when a target enters detection range.</summary>
    public override event Action<Transform> OnTargetDetectedEvent;

    /// <summary>Event raised when the target leaves detection range.</summary>
    public override event Action OnTargetLostEvent;
    #endregion

    #region Unity Messages
    private void Awake()
    {
        if (_collider == null)
        {
            _collider = GetComponent<SphereCollider>();
        }
    }

    private void OnDisable()
    {
        _playerTransform = null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (_playerTransform == null)
            {
                _playerTransform = other.transform;
                OnTargetDetectedEvent?.Invoke(_playerTransform);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _playerTransform = null;
            OnTargetLostEvent?.Invoke();
        }
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Initializes detection parameters.
    /// </summary>
    /// <param name="detectionRange">Perception radius to assign to the trigger collider.</param>
    public override void Initialize(float detectionRange)
    {
        if (_collider != null)
        {
            _collider.radius = detectionRange;
        }
    }

    /// <summary>
    /// Enables or disables target sensing.
    /// </summary>
    /// <param name="isEnabled">True to enable detection collider; false to disable perception.</param>
    public override void EnableDetect(bool isEnabled)
    {
        if (_collider != null)
        {
            _collider.enabled = isEnabled;
        }

        if (!isEnabled)
        {
            _playerTransform = null;
        }
    }
    #endregion
}
