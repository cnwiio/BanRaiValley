using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlantInteractor : MonoBehaviour, IInteractable
{
    [Header("REF")] [SerializeField]
    private Plant plant;

    
    
    [Header("Hover visual")] 
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private Material outlineMaterial;
    
    private readonly List<Material> _mat = new List<Material>();

    private void Awake()
    {
        meshRenderer.GetSharedMaterials(_mat);
    }
    
    public void Interact()
    {
        plant.Harvest();
    }

    public void IsLookAt(bool value)
    {
        if (plant.currentState != PlantState.ReadyToHarvest) return;
        if (value)
        {
            OnHover();
        }
        else
        {
            OnStopHover();
        }
    }

    private void OnHover()
    {
        _mat.Add(outlineMaterial);
        meshRenderer.SetSharedMaterials(_mat);
    }

    private void OnStopHover()
    {
        _mat.Remove(outlineMaterial);
        if (meshRenderer)
            meshRenderer.SetSharedMaterials(_mat);
    }
}
