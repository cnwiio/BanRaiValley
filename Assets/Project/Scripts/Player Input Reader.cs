using System;
using UnityEngine;
using UnityEngine.InputSystem;

public enum ActionMapType
{
    Player,
    UI
}

public class PlayerInputReader : MonoBehaviour
{
    // return value
    public Vector2 MovementInput { get; private set;  }

    [Header("Input Action Asset Reference")]
    [SerializeField] private InputActionAsset inputActionAsset;
    [SerializeField] private String PlayerActionMapName;
    [SerializeField] private String UIActionMapName;

    [Header("Input Action Reference")]
    [SerializeField] private InputActionReference MoveAction;
    [SerializeField] private InputActionReference JumpAction;
    [SerializeField] private InputActionReference InventoryToggleAction_PlayerMap;
    [SerializeField] private InputActionReference InventoryToggleAction_UIMap;
    [SerializeField] private InputActionReference HotbarSelectAction;
    [SerializeField] private InputActionReference HotbarScrollAction;
    [SerializeField] private InputActionReference PrimaryAction;
    [SerializeField] private InputActionReference SecondaryAction;

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
            }
        }
    }
    private InputActionMap playerActionMap;
    private InputActionMap UIActionMap;

    private void OnEnable()
    {
        playerActionMap = inputActionAsset.FindActionMap(PlayerActionMapName);
        UIActionMap = inputActionAsset.FindActionMap(UIActionMapName);

        MoveAction.action.performed += OnMove;
        MoveAction.action.canceled += OnStopMove;

        JumpAction.action.performed += OnJump;

        InventoryToggleAction_PlayerMap.action.performed += OnInventoryToggle;
        InventoryToggleAction_UIMap.action.performed += OnInventoryToggle;

        HotbarSelectAction.action.performed += OnHotBarSelect;

        HotbarScrollAction.action.performed += OnHotBarScroll;

        PrimaryAction.action.performed += OnPrimaryAction;

        SecondaryAction.action.performed += OnSecondaryAction;

        EventBus<ChangeActionMap>.Subscribe(ChangeActionMap);

        SwitchToPlayerActionMap();
    }

    private void OnDisable()
    {
        MoveAction.action.performed -= OnMove;
        MoveAction.action.canceled -= OnStopMove;

        JumpAction.action.performed -= OnJump;

        InventoryToggleAction_PlayerMap.action.performed -= OnInventoryToggle;
        InventoryToggleAction_UIMap.action.performed -= OnInventoryToggle;

        HotbarSelectAction.action.performed -= OnHotBarSelect;

        HotbarScrollAction.action.performed -= OnHotBarScroll;

        PrimaryAction.action.performed -= OnPrimaryAction;

        SecondaryAction.action.performed -= OnSecondaryAction;

        EventBus<ChangeActionMap>.Unsubscribe(ChangeActionMap);

        UIActionMap?.Disable();
        playerActionMap?.Disable();
        inputActionAsset.Disable();
    }

    public void ChangeActionMap(ChangeActionMap evt)
    {
        CurrentActionMaptype = evt.MapType;
    }

    public void SwitchToPlayerActionMap()
    {
        UIActionMap?.Disable();
        playerActionMap?.Enable();
    }

    public void SwitchToUIActionMap()
    {
        playerActionMap?.Disable();
        UIActionMap?.Enable();
    }

    
    private void OnInventoryToggle(InputAction.CallbackContext ctx)
    {
        EventBus<InventoryToggleEvent>.Raise(new InventoryToggleEvent() { });
    }

    private void OnHotBarSelect(InputAction.CallbackContext ctx)
    {
        EventBus<OnHotbarSelectEvent>.Raise(new OnHotbarSelectEvent() { Index = (int)ctx.ReadValue<float>() });
    }
    private void OnHotBarScroll(InputAction.CallbackContext ctx)
    {
        EventBus<OnHotbarScrollActionEvent>.Raise(new OnHotbarScrollActionEvent() { value = (int)ctx.ReadValue<float>() });
    }
    private void OnMove(InputAction.CallbackContext ctx)
    {
        MovementInput = ctx.ReadValue<Vector2>();
    }
    private void OnStopMove(InputAction.CallbackContext ctx)
    {
        MovementInput = Vector2.zero;
    }

    private void OnJump(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
            EventBus<OnJumpEvent>.Raise(new OnJumpEvent() { });
    }

    private void OnPrimaryAction(InputAction.CallbackContext ctx)
    {
        EventBus<OnPrimaryActionEvent>.Raise(new OnPrimaryActionEvent() { });
    }

    private void OnSecondaryAction(InputAction.CallbackContext ctx)
    {
        EventBus<OnSecondaryActionEvent>.Raise(new OnSecondaryActionEvent() { });
    }
}
