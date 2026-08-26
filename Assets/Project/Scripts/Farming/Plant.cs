using Lean.Pool;
using UnityEngine;

public enum PlantState
{
    CannotHarvest,
    ReadyToHarvest
}

public class Plant : MonoBehaviour, IInteractable, IPoolable
{
    private PlantState _currentState = PlantState.CannotHarvest;
    private int age;
    private PlantData data;

    public void Initialize(PlantData plantData)
    {
        data = plantData;
        Debug.Log("Initialize" + data);
    }
    
    public void Grow()
    {
        age++;
    }

    public void Interact()
    {
        if (_currentState != PlantState.ReadyToHarvest) return;
        // throw new System.NotImplementedException();
    }

    public void IsLookAt(bool value)
    {
        if (_currentState != PlantState.ReadyToHarvest) return;
        // throw new System.NotImplementedException();
    }

    public void OnSpawn()
    {
        age = 0;
    }

    public void OnDespawn()
    {
        // throw new System.NotImplementedException();
    }
}
