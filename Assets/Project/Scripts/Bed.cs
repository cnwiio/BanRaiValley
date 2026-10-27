using System;
using System.Collections;
using System.Collections.Generic;
using Project.Scripts;
using TMPro;
using UnityEngine;

public class Bed : MonoBehaviour, IInteractable
{
    [Header("TextUI")]
    [SerializeField] private TextTipReference textTipReference;
    [SerializeField] private String tipText;
    [Header("Outline")]
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private Material outlineMaterial;
    [Header("Sleep Ref")]
    [SerializeField] private SleepSequenceController sleepController;
    
    private ITextTip _textTip;
    private List<Material> mats = new List<Material>();
    private void Start()
    {
        _textTip ??= textTipReference.TextTip;

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
        _textTip.SetText(tipText);
        _textTip.SetActive(true);
        
        mats.Add(outlineMaterial);
        if (meshRenderer)
            meshRenderer.SetSharedMaterials(mats);
    }

    private void OnStopHover()
    {
        _textTip.SetActive(false);

        mats.Remove(outlineMaterial);
        if (meshRenderer)
            meshRenderer.SetSharedMaterials(mats);
    }
}