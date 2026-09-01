using System;
using System.Collections.Generic;
using Project.Scripts;
using UnityEngine;

public class CasinoInteractor : MonoBehaviour, IInteractable
{
    [Header("TextUI")]
    [SerializeField] private TextTipReference textTipReference;
    [SerializeField] private String tipText;
    [Header("Outline")]
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private Material outlineMaterial;
    [Header("UI Ref")] 
    [SerializeField] private GameObject ui;
    
    
    private ITextTip _textTip;
    private List<Material> mats = new List<Material>();
    private void OnEnable()
    {
        EventBus<InventoryToggleEvent>.Subscribe(OnInventoryToggle);
    }

    private void OnDisable()
    {
        EventBus<InventoryToggleEvent>.Unsubscribe(OnInventoryToggle);
    }
    private void OnInventoryToggle(InventoryToggleEvent evt)
    {
        if (ui.activeSelf)
            SetUIActive(false);
    }
    private void Start()
    {
        _textTip ??= textTipReference.TextTip;

        meshRenderer.GetSharedMaterials(mats);
    }
    public void Interact()
    {
        SetUIActive(true);
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

    public void SetUIActive(bool isActive)
    {
        ui.SetActive(isActive);
        
        if (isActive)
        {
            EventBus<ChangeActionMap>.Raise(new ChangeActionMap(){MapType = ActionMapType.UI});
        }
        else
        {
            EventBus<ChangeActionMap>.Raise(new ChangeActionMap(){MapType = ActionMapType.Player});
        }
    }   
}
