using System;
using System.Collections.Generic;
using Project.Scripts;
using UnityEngine;

public class ShopUIHandler : MonoBehaviour, IInteractable
{
    [Header("Shop UI")]
    [SerializeField] private GameObject shopUIPanel;

    [Header("Interactor Ref")]
    [SerializeField] private Material outlineMaterial;
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private TextTipReference textTipReference;
    [SerializeField] private String tipText;
    

    private readonly List<Material> _mats = new List<Material>();
    private ITextTip _textTip;

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
        if (shopUIPanel.activeSelf)
            SetUIActive(false);
    }
    private void Start()
    {
        meshRenderer.GetSharedMaterials(_mats);
        _textTip = textTipReference.TextTip;
    }

    public void SetUIActive(bool value)
    {
        shopUIPanel.SetActive(value);
        
        if (value)
        {
            EventBus<ChangeActionMap>.Raise(new ChangeActionMap(){MapType = ActionMapType.UI});
        }
        else
        {
            EventBus<ChangeActionMap>.Raise(new ChangeActionMap(){MapType = ActionMapType.Player});
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
        _textTip.SetText(tipText);
        _textTip.SetActive(true);
        
        _mats.Add(outlineMaterial);
        if (meshRenderer)
            meshRenderer.SetSharedMaterials(_mats);
    }

    private void OnStopHover()
    {
        _textTip.SetActive(false);
        
        _mats.Remove(outlineMaterial);
        if (meshRenderer)
            meshRenderer.SetSharedMaterials(_mats);
    }
}
