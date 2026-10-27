using System;
using Lean.Pool;
using UnityEngine;

public class Dirt : MonoBehaviour, IPoolable
{
    [SerializeField] private Material initialMaterial;
    [SerializeField] private Material wateredMaterial;
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private AwakenedCropHarvestTrigger plant;

    public void OnSpawn()
    {
        meshRenderer.sharedMaterial = initialMaterial;
    }

    public void OnDespawn()
    {
        // plant.Initialize(Vector3Int.zero);
        // plant.TriggerAwakening();
    }

    public void ResetWateredVisual()
    {
        meshRenderer.sharedMaterial = initialMaterial;
    }

    public void Watering()
    {
        meshRenderer.sharedMaterial = wateredMaterial;
    }
}
