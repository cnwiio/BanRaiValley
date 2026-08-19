using System;
using UnityEngine;

public class AIDetection : MonoBehaviour, IDetection
{
    private IWalkable move;
    private Transform playerTransform;

    public event Action<Transform> OnTargetDetectedEvent;
    public event Action OnTargetLostEvent;

    public void Initialize(IWalkable movement)
    {
        move = movement;
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
