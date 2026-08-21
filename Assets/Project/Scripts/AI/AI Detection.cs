using System;
using UnityEngine;

public class AIDetection : MonoBehaviour
{
    [SerializeField] private SphereCollider _collider;
    
    private Transform playerTransform;
    public bool IsPlayerInSight => playerTransform != null;

    public event Action<Transform> OnTargetDetectedEvent;
    public event Action OnTargetLostEvent;

    public void Initialize(float detectionRange)
    {
        // move = movement;
        _collider.radius = detectionRange;
    }

    public void EnableDetect(bool value)
    {
        _collider.enabled = value;
        if (!value) playerTransform = null;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (playerTransform == null)
            {
                playerTransform = other.transform;
                OnTargetDetectedEvent?.Invoke(playerTransform);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerTransform = null;
            OnTargetLostEvent?.Invoke();
        }
    }
}
