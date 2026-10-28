using System;
using System.Collections.Generic;
using Project.Scripts;
using UnityEngine;

public class CasinoInteractor : MonoBehaviour, IInteractable, ICloseableUI
{
    [Header("TextUI")]
    [SerializeField] private TextTipReference textTipReference;
    [SerializeField] private String tipText;
    [Header("Outline")]
    [SerializeField] private MeshRenderer[] meshRenderer;
    [SerializeField] private Material outlineMaterial;
    [Header("UI Ref")] 
    [SerializeField] private GameObject ui;
    
    private ITextTip _textTip;
    private List<Material> mats = new List<Material>();

    public bool IsOpen => ui != null && ui.activeSelf;

    private void OnEnable()
    {
        EventBus<InventoryToggleEvent>.Subscribe(OnInventoryToggle);
        EventBus<OnUIInteractActionEvent>.Subscribe(OnInteract);
    }

    private void OnDisable()
    {
        EventBus<InventoryToggleEvent>.Unsubscribe(OnInventoryToggle);
        EventBus<OnUIInteractActionEvent>.Unsubscribe(OnInteract);
        UIStackManager.UnregisterClosed(this);
    }

    private void OnInventoryToggle(InventoryToggleEvent evt)
    {
        if (IsOpen)
            SetUIActive(false);
    }

    private void OnInteract(OnUIInteractActionEvent evt)
    {
        if (IsOpen)
            SetUIActive(false);
    }

    public void CloseUI()
    {
        SetUIActive(false);
    }

    private void Start()
    {
        _textTip ??= textTipReference.TextTip;

        if (meshRenderer != null && meshRenderer.Length > 0 && meshRenderer[0] != null)
        {
            meshRenderer[0].GetSharedMaterials(mats);
        }
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
        _textTip?.SetText(tipText);
        _textTip?.SetActive(true);
        
        mats.Add(outlineMaterial);
        if (meshRenderer != null)
        {
            foreach (var renderer in meshRenderer)
            {
                if (renderer != null)
                    renderer.SetSharedMaterials(mats);
            }
        }
    }

    private void OnStopHover()
    {
        _textTip?.SetActive(false);

        mats.Remove(outlineMaterial);

        if (meshRenderer != null)
        {
            foreach (var renderer in meshRenderer)
            {
                if (renderer != null)
                    renderer.SetSharedMaterials(mats);
            }
        }
    }

    public void SetUIActive(bool isActive)
    {
        if (ui == null) return;

        ui.SetActive(isActive);
        
        if (isActive)
        {
            UIStackManager.RegisterOpen(this);
        }
        else
        {
            UIStackManager.UnregisterClosed(this);
        }
    }   
}
