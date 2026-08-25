using System;
using TMPro;
using UnityEngine;

public class Bed : MonoBehaviour, IInteractable
{
    [SerializeField] private TextMeshProUGUI tipTextUI;
    [SerializeField] private String tipText;

    private GameObject uiGameObject;
    private void Awake()
    {
        uiGameObject = tipTextUI.gameObject;
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
    }

    public void OnStopHover()
    {
        uiGameObject.SetActive(false);
    }
}