using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Bed : MonoBehaviour, IInteractable
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI tipTextUI;
    [SerializeField] private String tipText;
    [Header("Outline")]
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private Material outlineMaterial;

    private GameObject uiGameObject;
    private List<Material> mats = new List<Material>();
    private void Awake()
    {
        uiGameObject = tipTextUI.gameObject;
        meshRenderer.GetSharedMaterials(mats);
    }

    public void Interact()
    {
        Debug.Log(gameObject.name);
    }

    public void IsLookAt(bool value)
    {
        if (value)
        {
            OnHover();    
        }
        else
        {
            OnStopHover();
        }
        Debug.Log("Is player look at? = " + value);
    }

    public void OnHover()
    {
        tipTextUI.text = tipText;
        uiGameObject.SetActive(true);
        
        mats.Add(outlineMaterial);
        meshRenderer.SetSharedMaterials(mats);
    }

    public void OnStopHover()
    {
        uiGameObject.SetActive(false);

        mats.Remove(outlineMaterial);
        meshRenderer.SetSharedMaterials(mats);
    }
}