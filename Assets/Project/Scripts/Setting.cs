using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Setting : MonoBehaviour, ICloseableUI
{
    [SerializeField] private GameObject UiGameObject;
    [SerializeField] private InputActionReference lookAction;
    [SerializeField] private Slider slider;
    [SerializeField] private TextMeshProUGUI sliderValueText;
    
    public float baseSpeedX = 0.1f; 
    public float baseSpeedY = -0.1f;

    public bool IsOpen => UiGameObject != null && UiGameObject.activeSelf;

    private void Start()
    {
        if (slider != null && sliderValueText != null)
        {
            sliderValueText.SetText(slider.value.ToString());
            SetMouseSensitive(slider.value);
        }
    }

    private void OnEnable()
    {
        EventBus<OnEscapeActionEvent>.Subscribe(OnEscapeAction);
    }

    private void OnDisable()
    {
        EventBus<OnEscapeActionEvent>.Unsubscribe(OnEscapeAction);
        UIStackManager.UnregisterClosed(this);
    }

    private void OnEscapeAction(OnEscapeActionEvent evt)
    {
        if (UIStackManager.TryCloseTopUI())
        {
            return;
        }

        OpenPauseUI();
    }
    
    public void OpenPauseUI()
    {
        if (UiGameObject == null) return;

        UiGameObject.SetActive(true);
        UIStackManager.RegisterOpen(this);
    }

    public void CloseUI()
    {
        if (UiGameObject == null) return;

        UiGameObject.SetActive(false);
        UIStackManager.UnregisterClosed(this);
    }

    public void TogglePauseUI()
    {
        if (IsOpen)
        {
            CloseUI();
        }
        else
        {
            OpenPauseUI();
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

    public void Exit()
    {
        Application.Quit();
    }
}
