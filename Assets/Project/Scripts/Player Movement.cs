using NUnit.Framework.Internal.Commands;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Camera PlayerCam;
    [SerializeField] private float Speed;
    [SerializeField] private float JumpHeight;
    [SerializeField] private float GravityMultiplyer;

    private Vector3 movementInput;
    private Vector3 horizontalMovement;
    private float VerticalMovement;
    private Vector3 FinalMovement;
    private bool JumpTriggered = false;

    private Transform _camTransform;
    private Vector3 CamForwardDirection;
    private Vector3 CamRightDirection;

    private InputActionMap playerActionsMap;
    #region Handle Input


    public void OnMove(InputAction.CallbackContext value)
    {
        movementInput.x = value.ReadValue<Vector2>().x;
        movementInput.z = value.ReadValue<Vector2>().y;
    }

    public void OnJump(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) 
        {
            if (characterController.isGrounded)
            {
                JumpTriggered = true;
            }
        }
    }

    #endregion

    private void Awake()
    {
        playerActionsMap = InputSystem.actions.FindActionMap("Player");
        _camTransform = PlayerCam.transform;
    }

    private void Start()    
    {
        InputSystem.actions.FindActionMap("Player")?.Disable();
        playerActionsMap.Enable();

        // cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        CalculateRotation();
    }

    // Update is called once per frame
    void FixedUpdate()
    { 
        HandleVerticalMovement();
        HandleHorizontalMovement();
        ApplyMovement();
    }

    void HandleHorizontalMovement()
    {
        horizontalMovement = (CamForwardDirection * movementInput.z) + (CamRightDirection * movementInput.x);
        horizontalMovement *= Speed;
    }

    void HandleVerticalMovement()
    {
        if (characterController.isGrounded)
        {
            if (characterController.velocity.y < 0)
            {
                VerticalMovement = -0.5f; 
            }

            if (JumpTriggered)
            {
                VerticalMovement = Mathf.Sqrt(2 * Mathf.Abs(Physics.gravity.y) * JumpHeight);
                JumpTriggered = false;
            }
        }
        else
        {
            VerticalMovement += Physics.gravity.y * GravityMultiplyer;
        }
    }

    void ApplyMovement()
    {
        FinalMovement = horizontalMovement;
        FinalMovement.y = VerticalMovement;

        characterController.Move(FinalMovement * Time.fixedDeltaTime);
    }

    void CalculateRotation()
    {
        CamForwardDirection = _camTransform.forward;
        CamForwardDirection.y = 0;
        CamForwardDirection.Normalize();

        CamRightDirection = _camTransform.right;
        CamRightDirection.y = 0;
        CamRightDirection.Normalize();
    }
}
