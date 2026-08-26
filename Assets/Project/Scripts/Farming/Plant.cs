using System;
using System.Collections.Generic;
using Lean.Pool;
using TMPro;
using UnityEngine;

public enum PlantState
{
    CannotHarvest,
    ReadyToHarvest
}

public class Plant : MonoBehaviour, IPoolable
{
    [SerializeField] private MeshFilter meshFillter;

    
    
    
    private PlantState _currentState = PlantState.CannotHarvest;
    public PlantState currentState => _currentState;
    private PlantData data;
    public Vector3Int cellPos;


    public void Initialize(PlantData plantData, Vector3Int cellPos)
    {
        data = plantData;
        this.cellPos = cellPos;
        meshFillter.sharedMesh = data.Stages[_currentGrowStages].StageVisualMesh;
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

    public void Harvest()
    {
        if (_currentState != PlantState.ReadyToHarvest) return;
        // LeanPool.Despawn(this);
        // Vector3Int pos = new Vector3Int((int)transform.position.x, (int)transform.position.y, (int)transform.position.z);
        // EventBus<OnHarvestPlantEvent>.Raise(new OnHarvestPlantEvent(){Plant = this, CellPos = pos});
    }

    public void OnSpawn()
    {
        _currentStagesDays = 0;
        _currentGrowStages = 0;
        _currentState = PlantState.CannotHarvest;
    }

    public void OnDespawn()
    {
        data = null;
    }
}
