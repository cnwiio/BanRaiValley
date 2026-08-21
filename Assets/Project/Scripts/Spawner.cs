using System;
using Lean.Pool;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private int Amount;
    [SerializeField] private Transform spawnTransform;

    private void OnEnable()
    {
        EventBus<OnJumpEvent>.Subscribe(OnSpace);
    }

    private void OnDisable()
    {
        EventBus<OnJumpEvent>.Unsubscribe(OnSpace);
    }

    private void OnSpace(OnJumpEvent evt)
    {
        Spawn();
    }
    
    [ContextMenu("Spawn")]
    void Spawn()
    {
        for (int i = 0; i < Amount; i++)
        {
            LeanPool.Spawn(prefab, spawnTransform.position, transform.rotation);
        }
    }
}
