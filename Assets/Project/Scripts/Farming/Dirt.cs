using System;
using Lean.Pool;
using UnityEngine;

public class Dirt : MonoBehaviour, IPoolable
{
    [SerializeField] private Material intialMaterial;
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private AwakenedCropHarvestTrigger plant;

    public void OnSpawn()
    {
        meshRenderer.sharedMaterial = intialMaterial;
    }

    public void OnDespawn()
    {
        plant.Initialize(Vector3Int.zero);
        plant.TriggerAwakening();
    }
}
