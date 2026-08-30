using System.Collections.Generic;
using Project.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class PlantInteractor : MonoBehaviour, IInteractable
{
    [Header("REF")] [SerializeField]
    private Plant plant;
    
    [Header("Text Tip Ref")] 
    [SerializeField] private TextTipReference textTipReference;
    [SerializeField] private string tipText;
    

    
    [Header("Hover visual")] 
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private Material outlineMaterial;
    
    private readonly List<Material> _mat = new List<Material>();
    private ITextTip _textTip;

    private void Awake()
    {
        meshRenderer.GetSharedMaterials(_mat);
        _textTip = textTipReference.TextTip;
    }
    
    public void Interact()
    {
        plant.Harvest();
    }

    public void IsLookAt(bool value)
    {
        if (plant.currentState != PlantState.ReadyToHarvest &&
            plant.currentState != PlantState.Withered) return;
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
        _textTip.SetText(tipText);
        _textTip.SetActive(true);
        
        _mat.Add(outlineMaterial);
        if (meshRenderer)
            meshRenderer.SetSharedMaterials(_mat);
    }

    private void OnStopHover()
    {
        _textTip.SetActive(false);
        
        _mat.Remove(outlineMaterial);
        if (meshRenderer)
            meshRenderer.SetSharedMaterials(_mat);
    }
}
