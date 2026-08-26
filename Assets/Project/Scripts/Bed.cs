using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Bed : MonoBehaviour, IInteractable
{
    [Header("TextUI")]
    [SerializeField] private TextMeshProUGUI tipTextUI;
    [SerializeField] private String tipText;
    [Header("Outline")]
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private Material outlineMaterial;
    [Header("Sleep Ref")]
    [SerializeField] private SleepSequenceController sleepController;
    
    private GameObject uiGameObject;
    private List<Material> mats = new List<Material>();
    private void Awake()
    {
        uiGameObject = tipTextUI.gameObject;
        meshRenderer.GetSharedMaterials(mats);
    }

    public void Interact()
    {
        sleepController.HandleSleep();
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
    }

    private void OnHover()
    {
        tipTextUI.text = tipText;
        uiGameObject.SetActive(true);
        
        mats.Add(outlineMaterial);
        meshRenderer.SetSharedMaterials(mats);
    }

    private void OnStopHover()
    {
        uiGameObject.SetActive(false);

        mats.Remove(outlineMaterial);
        meshRenderer.SetSharedMaterials(mats);
    }
}