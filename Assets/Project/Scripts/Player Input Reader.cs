using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputReader : MonoBehaviour
{
    public Vector2 MovementInput { get; private set;  }

    [SerializeField] private InputActionReference MoveAction;
    [SerializeField] private InputActionReference JumpAction;

    private void OnEnable()
    {
        MoveAction.action.performed += OnMove;
        MoveAction.action.canceled += OnStopMove;
        MoveAction.action.Enable();

        JumpAction.action.performed += OnJump;
        JumpAction.action.Enable();
    }

    private void OnDisable()
    {
        MoveAction.action.performed -= OnMove;
        MoveAction.action.canceled -= OnStopMove;
        MoveAction.action.Disable();

        JumpAction.action.performed -= OnJump;
        JumpAction.action.Disable();
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

    private void Start()
    {
        // cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
