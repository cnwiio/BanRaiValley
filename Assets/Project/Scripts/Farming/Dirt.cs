using System;
using Lean.Pool;
using UnityEngine;

public class Dirt : MonoBehaviour, IPoolable
{
    [SerializeField] private Material intialMaterial;
    [SerializeField] private MeshRenderer meshRenderer;

    public void OnSpawn()
    {
        meshRenderer.sharedMaterial = intialMaterial;
    }

    public void OnDespawn()
    {
        // throw new System.NotImplementedException();
    }
}
