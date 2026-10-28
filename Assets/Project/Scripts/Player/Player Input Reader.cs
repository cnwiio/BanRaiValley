using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputReader : MonoBehaviour
{
    // return values
    public Vector2 MovementInput { get; private set; }
    public bool IsSprintPressed { get; private set; }
    public bool IsCrouchPressed { get; private set; }

    [SerializeField] private InputActionReference DebugAction;

    [Header("Input Action Reference")]
    [SerializeField] private InputActionReference MoveAction;
    [SerializeField] private InputActionReference JumpAction;
    [SerializeField] private InputActionReference RunAction;
    [SerializeField] private InputActionReference CrouchAction;
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
    [SerializeField] private InputActionReference EscapeAction_PlayerMap;
    [SerializeField] private InputActionReference EscapeAction_UIMap;

    private InputAction _activeRunAction;
    private InputAction _activeCrouchAction;

    private void OnEnable()
    {
        DebugAction.action.performed += OnDebug;

        MoveAction.action.performed += OnMove;
        MoveAction.action.canceled += OnStopMove;

        JumpAction.action.performed += OnJump;

        _activeRunAction = RunAction != null ? RunAction.action : MoveAction?.asset?.FindAction("Movement/Run") ?? MoveAction?.asset?.FindAction("Run");
        if (_activeRunAction != null)
        {
            _activeRunAction.performed += OnRun;
            _activeRunAction.canceled += OnStopRun;
        }

        _activeCrouchAction = CrouchAction != null ? CrouchAction.action : MoveAction?.asset?.FindAction("Movement/Crouch") ?? MoveAction?.asset?.FindAction("Crouch");
        if (_activeCrouchAction != null)
        {
            _activeCrouchAction.performed += OnCrouch;
            _activeCrouchAction.canceled += OnStopCrouch;
        }

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

        EscapeAction_PlayerMap.action.performed += OnEscapeAction;
        EscapeAction_UIMap.action.performed += OnEscapeAction;
    }

    private void OnDisable()
    {
        DebugAction.action.performed -= OnDebug;

        MoveAction.action.performed -= OnMove;
        MoveAction.action.canceled -= OnStopMove;

        JumpAction.action.performed -= OnJump;

        if (_activeRunAction != null)
        {
            _activeRunAction.performed -= OnRun;
            _activeRunAction.canceled -= OnStopRun;
            _activeRunAction = null;
        }

        if (_activeCrouchAction != null)
        {
            _activeCrouchAction.performed -= OnCrouch;
            _activeCrouchAction.canceled -= OnStopCrouch;
            _activeCrouchAction = null;
        }
        IsSprintPressed = false;
        IsCrouchPressed = false;

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

        EscapeAction_PlayerMap.action.performed -= OnEscapeAction;
        EscapeAction_UIMap.action.performed -= OnEscapeAction;
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

    private void OnRun(InputAction.CallbackContext ctx)
    {
        IsSprintPressed = true;
    }

    private void OnStopRun(InputAction.CallbackContext ctx)
    {
        IsSprintPressed = false;
    }

    private void OnCrouch(InputAction.CallbackContext ctx)
    {
        IsCrouchPressed = true;
    }

    private void OnStopCrouch(InputAction.CallbackContext ctx)
    {
        IsCrouchPressed = false;
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

    private void OnEscapeAction(InputAction.CallbackContext ctx)
    {
        EventBus<OnEscapeActionEvent>.Raise(new OnEscapeActionEvent());
    }
}
