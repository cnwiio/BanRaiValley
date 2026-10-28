using System;
using Lean.Pool;
using UnityEngine;

public class Dirt : MonoBehaviour, IPoolable
{
    [SerializeField] private Material initialMaterial;
    [SerializeField] private Material wateredMaterial;
    [SerializeField] private MeshRenderer meshRenderer;

    public int InactiveDays { get; private set; }

    public void OnSpawn()
    {
        meshRenderer.sharedMaterial = initialMaterial;
        InactiveDays = 0;
    }

    public void OnDespawn()
    {
        // throw new System.NotImplementedException();
    }

    public void ResetWateredVisual()
    {
        meshRenderer.sharedMaterial = initialMaterial;
    }

    public void Watering()
    {
        meshRenderer.sharedMaterial = wateredMaterial;
        ResetInactiveDays();
    }

    public void ResetInactiveDays()
    {
        InactiveDays = 0;
    }

    public void IncrementInactiveDays()
    {
        InactiveDays++;
    }
}
