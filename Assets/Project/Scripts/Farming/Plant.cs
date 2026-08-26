using System;
using System.Collections.Generic;
using Lean.Pool;
using UnityEngine;

public enum PlantState
{
    CannotHarvest,
    ReadyToHarvest
}

public class Plant : MonoBehaviour, IInteractable, IPoolable
{
    [SerializeField] private MeshFilter meshFillter;
    [Header("Hover visual")] 
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private Material outlineMaterial;
    
    private PlantState _currentState = PlantState.CannotHarvest;
    private PlantData data;

    private List<Material> _mat = new List<Material>();

    private void Awake()
    {
        meshRenderer.GetSharedMaterials(_mat);
    }

    public void Initialize(PlantData plantData)
    {
        data = plantData; 
        Debug.Log("Initialize" + data);
    }

    private byte _currentGrowStages;
    private byte _currentStagesDays;
    [ContextMenu("Grow")]
    public void Grow()
    {
        if (_currentGrowStages == data.FinalStageIndex) return;
        
        _currentStagesDays++;
        
        if (_currentStagesDays >= data.Stages[_currentGrowStages].DaysRequired)
        {
            _currentGrowStages++;
            meshFillter.sharedMesh = data.Stages[_currentGrowStages].StageVisualMesh;
            if (_currentGrowStages == data.FinalStageIndex)
            {
                _currentState = PlantState.ReadyToHarvest;
            }
        }
        
    }

    public void Interact()
    {
        if (_currentState != PlantState.ReadyToHarvest) return;
        Debug.Log("Harvest");
        // throw new System.NotImplementedException();
    }

    public void IsLookAt(bool value)
    {
        if (_currentState != PlantState.ReadyToHarvest) return;
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
        meshRenderer.SetSharedMaterials(_mat);
    }

    public void OnSpawn()
    {
        _currentStagesDays = 0;
        _currentGrowStages = 0;
        _currentState = PlantState.CannotHarvest;
        if (!data) return;
        meshFillter.sharedMesh = data.Stages[_currentGrowStages].StageVisualMesh;
    }

    public void OnDespawn()
    {
        // throw new System.NotImplementedException();
    }
}
