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
    [Header("Black Out UI")]
    [SerializeField] private CanvasGroup blackOutUI;
    [Header("Outline")]
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private Material outlineMaterial;

    [SerializeField] private Transform awakePosition;
    
    

    private GameObject uiGameObject;
    private List<Material> mats = new List<Material>();
    private bool db;
    private void Awake()
    {
        uiGameObject = tipTextUI.gameObject;
        meshRenderer.GetSharedMaterials(mats);
    }

    public void Interact()
    {
        Debug.Log(gameObject.name);
        // StartCoroutine(BlackOutCoroutine());
        EventBus<OnSleepEvent>.Raise(new OnSleepEvent(){AwakeTransform = awakePosition});
        // EventBus<ChangeActionMap>.Raise(new ChangeActionMap(){MapType = ActionMapType.Static});
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