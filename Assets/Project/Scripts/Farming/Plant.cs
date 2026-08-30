using System;
using System.Collections.Generic;
using Lean.Pool;
using TMPro;
using UnityEngine;

public enum PlantState
{
    CannotHarvest,
    ReadyToHarvest,
    Withered
}

public class Plant : MonoBehaviour, IPoolable
{
    [SerializeField] private MeshFilter meshFilter;
    
    
    private PlantState _currentState = PlantState.CannotHarvest;
    public PlantState currentState => _currentState;
    private PlantData data;


    public void Initialize(PlantData plantData)
    {
        data = plantData;
        meshFilter.sharedMesh = data.Stages[_currentGrowStages].StageVisualMesh;
    }

    private byte _currentGrowStages;
    private float _currentStagesDays;
    private float _currentDeathDays;
    [ContextMenu("Grow")]
    public void Grow()
    {
        if (_currentGrowStages >= data.FinalStageIndex) return;
        
        _currentDeathDays = 0;
        _currentStagesDays++;
        
        if (_currentStagesDays >= data.Stages[_currentGrowStages].DaysRequired)
        {
            _currentGrowStages++;
            meshFilter.sharedMesh = data.Stages[_currentGrowStages].StageVisualMesh;
            if (_currentGrowStages >= data.FinalStageIndex)
            {
                _currentState = PlantState.ReadyToHarvest;
            }
        }
        
    }

    public void Withered()
    {
        _currentDeathDays++;
        _currentStagesDays += 0.5f;

        if (_currentDeathDays >= data.DeathStages.DaysRequired)
        {
            meshFilter.sharedMesh = data.DeathStages.StageVisualMesh;
            _currentState = PlantState.Withered;
        }
    }

    public void Harvest()
    {
        if (_currentState == PlantState.Withered)
        {
            EventBus<OnClearPlantEvent>.Raise(new OnClearPlantEvent(){ IsWithered = true, Pos = transform.position });
            return;
        }
        
        if (_currentState != PlantState.ReadyToHarvest) return;
        LeanPool.Spawn(data.plantMonsterPrefabs, transform.position, transform.rotation);
        EventBus<OnHarvestPlantEvent>.Raise(new OnHarvestPlantEvent() { Data = data, Position = transform.position});
    }

    public void OnSpawn()
    {
        _currentStagesDays = 0;
        _currentGrowStages = 0;
        _currentDeathDays = 0;
        _currentState = PlantState.CannotHarvest;
    }

    public void OnDespawn()
    {
        data = null;
    }
}
