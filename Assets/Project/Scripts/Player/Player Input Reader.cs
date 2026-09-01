using System;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerInputReader : MonoBehaviour
{
    // return value
    public Vector2 MovementInput { get; private set;  }

    [SerializeField] private InputActionReference DebugAction;
    
    
    [Header("Input Action Reference")]
    [SerializeField] private InputActionReference MoveAction;
    [SerializeField] private InputActionReference JumpAction;
    [SerializeField] private InputActionReference InventoryToggleAction_PlayerMap;
    [SerializeField] private InputActionReference InventoryToggleAction_UIMap;
    [SerializeField] private InputActionReference HotbarSelectAction;
    [SerializeField] private InputActionReference HotbarScrollAction;
    [SerializeField] private InputActionReference PrimaryAction;
    [SerializeField] private InputActionReference SecondaryAction;
    [SerializeField] private InputActionReference RotateAction;
    [SerializeField] private InputActionReference DeleteAction;
    [SerializeField] private InputActionReference InteractAction_PlayerMap;
    [SerializeField] private InputActionReference InteractAction_UIMap;
    

    private void OnEnable()
    {
        DebugAction.action.performed += OnDebug;
        
        MoveAction.action.performed += OnMove;
        MoveAction.action.canceled += OnStopMove;

        JumpAction.action.performed += OnJump;

        InventoryToggleAction_PlayerMap.action.performed += OnInventoryToggle;
        InventoryToggleAction_UIMap.action.performed += OnInventoryToggle;

        HotbarSelectAction.action.performed += OnHotBarSelect;

        HotbarScrollAction.action.performed += OnHotBarScroll;

        PrimaryAction.action.performed += OnPrimaryAction;

        SecondaryAction.action.performed += OnSecondaryAction;

        RotateAction.action.performed += OnRotateAction;

        DeleteAction.action.performed += OnDeleteAction;

        InteractAction_PlayerMap.action.performed += OnInteractAction;
        InteractAction_UIMap.action.performed += OnUIInteractAction;
    }

    private void OnDisable()
    {
        DebugAction.action.performed -= OnDebug;
        
        MoveAction.action.performed -= OnMove;
        MoveAction.action.canceled -= OnStopMove;

        JumpAction.action.performed -= OnJump;

        InventoryToggleAction_PlayerMap.action.performed -= OnInventoryToggle;
        InventoryToggleAction_UIMap.action.performed -= OnInventoryToggle;

        HotbarSelectAction.action.performed -= OnHotBarSelect;

        HotbarScrollAction.action.performed -= OnHotBarScroll;

        PrimaryAction.action.performed -= OnPrimaryAction;

        SecondaryAction.action.performed -= OnSecondaryAction;

        RotateAction.action.performed -= OnRotateAction;

        DeleteAction.action.performed -= OnDeleteAction;
        
        InteractAction_PlayerMap.action.performed -= OnInteractAction;
        InteractAction_UIMap.action.performed -= OnUIInteractAction;
    }

    private void OnDebug(InputAction.CallbackContext ctx)
    {
        EventBus<OnDebugActionEvent>.Raise(new OnDebugActionEvent() { });
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

    private void OnRotateAction(InputAction.CallbackContext ctx)
    {
        EventBus<OnRotateActionEvent>.Raise(new OnRotateActionEvent() { });
    }

    private void OnDeleteAction(InputAction.CallbackContext ctx)
    {
        EventBus<OnDeleteActionEvent>.Raise(new OnDeleteActionEvent() { });
    }

    private void OnInteractAction(InputAction.CallbackContext ctx)
    {
        EventBus<OnInteractActionEvent>.Raise(new OnInteractActionEvent());
    }
    
    private void OnUIInteractAction(InputAction.CallbackContext ctx)
    {
        EventBus<OnUIInteractActionEvent>.Raise(new OnUIInteractActionEvent());
    }
}
