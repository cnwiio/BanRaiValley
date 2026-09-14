using System;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Setting : MonoBehaviour
{
    [SerializeField] private GameObject UiGameObject;
    [SerializeField] private InputActionReference lookAction;
    [SerializeField] private Slider slider;
    [SerializeField] private TextMeshProUGUI sliderValueText;
    
    
    
    // [SerializeField] private  inputActionReference;
    
    public float baseSpeedX = 0.1f; 
    public float baseSpeedY =-0.1f;


    private void Start()
    {
        sliderValueText.SetText(slider.value.ToString());
        SetMouseSensitive(slider.value);
    }

    private void OnEnable()
    {
        EventBus<OnEscapeActionEvent>.Subscribe(OnEscapeAction);
    }

    private void OnDisable()
    {
        EventBus<OnEscapeActionEvent>.Unsubscribe(OnEscapeAction);
    }

    private void OnEscapeAction(OnEscapeActionEvent evt)
    {
        // Debug.Log("yes");
        TogglePauseUI();
    }
    
    private void TogglePauseUI()
    {
        // if (UiGameObject.activeSelf)
            UiGameObject.SetActive(!UiGameObject.activeSelf);
            if (UiGameObject.activeSelf)
            {
                EventBus<ChangeActionMap>.Raise(new ChangeActionMap(){MapType = ActionMapType.UI});
            }
            else
            {
                EventBus<ChangeActionMap>.Raise(new ChangeActionMap(){MapType = ActionMapType.Player});
            }
    }

    public void SetMouseSensitive(float multiplier)
    {
        if (!lookAction) return;
        
        lookAction.action.ApplyParameterOverride("scaleVector2:x", baseSpeedX * multiplier);
        lookAction.action.ApplyParameterOverride("scaleVector2:y", baseSpeedY * multiplier);
        
        sliderValueText?.SetText(multiplier.ToString());
    }

    public void SetVsync(bool value)
    {
        QualitySettings.vSyncCount = value ? 1 : 0;
        if (!value)
        {
            Application.targetFrameRate = 0; 
        }
    }
}
