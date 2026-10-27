using System;
using UnityEngine;
using UnityEngine.InputSystem;

public enum ActionMapType
{
    Player,
    UI,
    Static
}
public class InputManager : MonoBehaviour
{
    [Header("Input Action Asset Reference")]
    [SerializeField] private InputActionAsset inputActionAsset;
    [SerializeField] private String PlayerActionMapName;
    [SerializeField] private String UIActionMapName;
    [SerializeField] private String HotbarActionMapName;
    [SerializeField] private String MovementActionMapName;
    
    private InputActionMap playerActionMap;
    private InputActionMap UIActionMap;
    private InputActionMap HotbarActionMap;
    private InputActionMap MovementActionMap;
    
    private ActionMapType currentActionMaptype;
    public ActionMapType CurrentActionMaptype
    {
        get => currentActionMaptype;
        set
        {
            currentActionMaptype = value;
            switch (value)
            {
                case ActionMapType.Player:
                    SwitchToPlayerActionMap();
                    break;
                case ActionMapType.UI:
                    SwitchToUIActionMap();
                    break;
                case ActionMapType.Static:
                    SwitchToStaticUIActionMap();
                    break;
            }
        }
    }

    private void OnEnable()
    {
        playerActionMap = inputActionAsset.FindActionMap(PlayerActionMapName);
        UIActionMap = inputActionAsset.FindActionMap(UIActionMapName);
        HotbarActionMap = inputActionAsset.FindActionMap(HotbarActionMapName);
        MovementActionMap = inputActionAsset.FindActionMap(MovementActionMapName);
        
        EventBus<ChangeActionMap>.Subscribe(ChangeActionMap);
        EventBus<OnStartTillingEvent>.Subscribe(OnStartTilling);
        EventBus<OnTillingImpactEvent>.Subscribe(OnTillingImpact);
        EventBus<OnStartWateringEvent>.Subscribe(OnStartWatering);
        EventBus<OnWateringEvent>.Subscribe(OnWatering);
        EventBus<OnStartPlantingEvent>.Subscribe(OnStartPlanting);
        EventBus<OnPlantingEvent>.Subscribe(OnPlanting);
        
        CurrentActionMaptype = ActionMapType.Player;
    }

    private void OnDisable()
    {
        EventBus<ChangeActionMap>.Unsubscribe(ChangeActionMap);
        EventBus<OnStartTillingEvent>.Unsubscribe(OnStartTilling);
        EventBus<OnTillingImpactEvent>.Unsubscribe(OnTillingImpact);
        EventBus<OnWateringEvent>.Unsubscribe(OnWatering);
        EventBus<OnStartPlantingEvent>.Unsubscribe(OnStartPlanting);
        EventBus<OnPlantingEvent>.Unsubscribe(OnPlanting);
        
        UIActionMap?.Disable();
        playerActionMap?.Disable();
        HotbarActionMap?.Disable();
        MovementActionMap?.Disable();

        inputActionAsset.Disable();
    }

    private void ChangeActionMap(ChangeActionMap evt)
    {
        CurrentActionMaptype = evt.MapType;
    }
    private void OnStartTilling(OnStartTillingEvent evt)
    {
        HotbarActionMap?.Disable();
        MovementActionMap?.Disable();
    }

    private void OnTillingImpact(OnTillingImpactEvent evt)
    {
        HotbarActionMap?.Enable();
        MovementActionMap?.Enable();
    }

    private void OnStartWatering(OnStartWateringEvent evt)
    {
        HotbarActionMap?.Disable();
    }

    private void OnWatering(OnWateringEvent evt)
    {
        HotbarActionMap?.Enable();
    }
    private void OnStartPlanting(OnStartPlantingEvent evt)
    {
        HotbarActionMap?.Disable();
    }
    private void OnPlanting(OnPlantingEvent evt)
    {
        HotbarActionMap?.Enable();
    }

    
    
    public void SwitchToPlayerActionMap()
    {
        UIActionMap?.Disable();

        playerActionMap?.Enable();
        MovementActionMap?.Enable();
        HotbarActionMap?.Enable();
        SetCursorState(false);
    }

    public void SwitchToUIActionMap()
    {
        playerActionMap?.Disable();
        MovementActionMap?.Disable();
        HotbarActionMap?.Disable();

        UIActionMap?.Enable();
        SetCursorState(true);
    }

    public void SwitchToStaticUIActionMap()
    {
        UIActionMap?.Disable();
        playerActionMap?.Disable();
        HotbarActionMap?.Disable();
        MovementActionMap?.Disable();
        SetCursorState(false);

    }
    
    void SetCursorState(bool isVisible)
    {
        Cursor.lockState = isVisible ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isVisible;
    }
}