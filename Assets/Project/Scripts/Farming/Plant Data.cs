using UnityEngine;

[CreateAssetMenu(fileName = "PlantData", menuName = "Scriptable Objects/PlantData")]
public class PlantData : ScriptableObject
{
    public GameObject prefabs;

    public PlantStageData[] Stages;
    
    public int FinalStageIndex => Stages.Length > 0 ? Stages.Length - 1 : 0;
}
